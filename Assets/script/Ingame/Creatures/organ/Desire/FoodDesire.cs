using UnityEngine;

public class FoodDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        FoodMemory memory = GetComponent<FoodMemory>();
        if (context == null || context.Transform == null || memory == null)
            return AIMoveIntent.None("food missing");

        Vector3 target = memory.rememberedFood != null
            ? memory.rememberedFood.transform.position
            : memory.rememberedPosition;

        Vector3 dir = target - context.Transform.position;
        dir.y = 0f;
        return new AIMoveIntent { direction = dir, weight = level, reason = "food" };
    }
}
