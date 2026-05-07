using UnityEngine;

public class CorpseEatAction : MonoBehaviour, IAIAction
{
    public float eatRadius = 1.2f;
    public float eatSpeed = 10f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.BodyResource == null || context.Transform == null)
            return false;

        Collider[] hits = Physics.OverlapSphere(context.Transform.position, eatRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].TryGetComponent<Resource>(out var resource)) continue;
            if (resource == context.BodyResource || resource.resourceCategory == category.grass) continue;
            if (!IsCorpse(hits[i].gameObject)) continue;
            context.BodyResource.Eating(eatSpeed * deltaTime, resource, "organ corpse eat");
            return true;
        }

        return false;
    }

    static bool IsCorpse(GameObject obj)
    {
        if (obj == null)
            return false;
        if (obj.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return herbivore.IsDead;
        if (obj.TryGetComponent<predatorBehaviour>(out var predator))
            return predator.IsDead;
        return false;
    }
}
