using UnityEngine;

public class GrassEatAction : MonoBehaviour, IAIAction
{
    public float eatRadius = 1f;
    public float eatSpeed = 10f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.BodyResource == null || context.Transform == null)
            return false;

        Collider[] hits = Physics.OverlapSphere(context.Transform.position, eatRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].TryGetComponent<Resource>(out var resource)) continue;
            if (resource.resourceCategory != category.grass) continue;
            context.BodyResource.Eating(eatSpeed * deltaTime, resource, "organ grass eat");
            return true;
        }

        return false;
    }
}
