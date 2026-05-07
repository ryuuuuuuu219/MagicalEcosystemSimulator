using System.Collections.Generic;
using UnityEngine;

public class FoodVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject bestFood;
    public float fallbackVisionDistance = 50f;
    public float fallbackVisionAngle = 35f;

    public void TickSense(AIContext context, float deltaTime)
    {
        bestFood = null;
        if (context == null || context.Transform == null)
            return;
        if (!TryGetComponent<herbivoreBehaviour>(out var herbivore) || herbivore.herbivoreManager == null)
            return;
        if (!herbivore.herbivoreManager.returngrasses(out List<GameObject> grasses) || grasses == null)
            return;

        float visionDistance = herbivore.genome.visionDistance > 0f ? herbivore.genome.visionDistance : fallbackVisionDistance;
        float visionAngle = herbivore.genome.visionAngle > 0f ? herbivore.genome.visionAngle : fallbackVisionAngle;
        bestFood = FindClosestVisible(context.Transform, grasses, visionDistance, visionAngle);

        if (bestFood != null && TryGetComponent<FoodMemory>(out var memory))
            memory.Remember(bestFood);
    }

    static GameObject FindClosestVisible(Transform origin, List<GameObject> candidates, float distance, float angle)
    {
        GameObject best = null;
        float bestDist = float.MaxValue;
        Vector3 eye = origin.position + Vector3.up * 0.6f;
        float halfAngle = Mathf.Max(0f, angle);

        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject candidate = candidates[i];
            if (candidate == null) continue;

            Vector3 to = candidate.transform.position - eye;
            float dist = to.magnitude;
            if (dist <= 0.001f || dist > distance || dist >= bestDist) continue;
            if (Vector3.Angle(origin.forward, to / dist) > halfAngle) continue;

            best = candidate;
            bestDist = dist;
        }

        return best;
    }
}
