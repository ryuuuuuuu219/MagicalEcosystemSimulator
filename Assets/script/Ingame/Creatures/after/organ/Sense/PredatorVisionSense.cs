using System.Collections.Generic;
using UnityEngine;

public class PredatorVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject nearestPredator;
    public float fallbackDetectDistance = 40f;

    public void TickSense(AIContext context, float deltaTime)
    {
        nearestPredator = null;
        if (context == null || context.Transform == null)
            return;
        if (!TryGetComponent<herbivoreBehaviour>(out var herbivore) || herbivore.herbivoreManager == null)
            return;
        if (!herbivore.herbivoreManager.returnPredators(out List<GameObject> predators) || predators == null)
            return;

        float detectDistance = herbivore.genome.threatDetectDistance > 0f
            ? herbivore.genome.threatDetectDistance
            : fallbackDetectDistance;
        nearestPredator = FindNearest(context.Transform.position, predators, detectDistance, gameObject);

        if (nearestPredator != null && TryGetComponent<ThreatMemory>(out var memory))
            memory.Remember(nearestPredator);
    }

    static GameObject FindNearest(Vector3 origin, List<GameObject> candidates, float maxDistance, GameObject self)
    {
        GameObject best = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject candidate = candidates[i];
            if (candidate == null || candidate == self) continue;

            float dist = Vector3.Distance(origin, candidate.transform.position);
            if (dist > maxDistance || dist >= bestDist) continue;
            best = candidate;
            bestDist = dist;
        }

        return best;
    }
}
