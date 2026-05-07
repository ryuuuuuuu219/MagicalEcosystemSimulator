using System.Collections.Generic;
using UnityEngine;

public class ThreatVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject nearestThreat;
    public float fallbackDetectDistance = 40f;

    public void TickSense(AIContext context, float deltaTime)
    {
        nearestThreat = null;
        if (context == null || context.Transform == null)
            return;
        if (!TryGetComponent<predatorBehaviour>(out var predator) || predator.predatorManager == null)
            return;
        if (!predator.predatorManager.returnThreats(out List<GameObject> threats) || threats == null)
            return;

        float detectDistance = predator.genome.threatDetectDistance > 0f
            ? predator.genome.threatDetectDistance
            : fallbackDetectDistance;
        nearestThreat = FindNearest(context.Transform.position, threats, detectDistance, gameObject);

        if (nearestThreat != null && TryGetComponent<ThreatMemory>(out var memory))
            memory.Remember(nearestThreat);
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
