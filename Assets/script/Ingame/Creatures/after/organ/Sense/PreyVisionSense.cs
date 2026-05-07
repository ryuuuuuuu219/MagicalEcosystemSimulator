using System.Collections.Generic;
using UnityEngine;

public class PreyVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject bestPrey;
    public float fallbackDetectDistance = 60f;

    public void TickSense(AIContext context, float deltaTime)
    {
        bestPrey = null;
        if (context == null || context.Transform == null || context.BodyResource == null)
            return;
        if (!TryGetComponent<predatorBehaviour>(out var predator) || predator.predatorManager == null)
            return;

        float detectDistance = predator.genome.preyDetectDistance > 0f
            ? predator.genome.preyDetectDistance
            : fallbackDetectDistance;

        bestPrey = FindBestPrey(context, predator.predatorManager, detectDistance);
        if (bestPrey != null && TryGetComponent<PreyMemory>(out var memory))
            memory.Remember(bestPrey);
        if (bestPrey != null && TryGetComponent<TargetTracker>(out var tracker))
            tracker.Track(bestPrey, deltaTime);
    }

    GameObject FindBestPrey(AIContext context, predatorManager manager, float maxDistance)
    {
        GameObject best = null;
        float bestDist = float.MaxValue;
        CreatureRelationResolver resolver = GetComponent<CreatureRelationResolver>();

        if (manager.returnHerbivores(out List<GameObject> herbivores))
            FindBestInList(context, herbivores, resolver, maxDistance, ref best, ref bestDist);

        FindBestInList(context, manager.predators, resolver, maxDistance, ref best, ref bestDist);
        return best;
    }

    void FindBestInList(
        AIContext context,
        List<GameObject> candidates,
        CreatureRelationResolver resolver,
        float maxDistance,
        ref GameObject best,
        ref float bestDist)
    {
        if (candidates == null) return;

        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject candidate = candidates[i];
            if (candidate == null || candidate == gameObject) continue;
            if (!candidate.TryGetComponent<Resource>(out var resource)) continue;
            if (resolver != null && !resolver.CanTargetAsPrey(context.BodyResource, resource)) continue;

            float dist = Vector3.Distance(context.Transform.position, candidate.transform.position);
            if (dist > maxDistance || dist >= bestDist) continue;

            best = candidate;
            bestDist = dist;
        }
    }
}
