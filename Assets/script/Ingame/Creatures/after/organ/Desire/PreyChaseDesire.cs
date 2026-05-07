using UnityEngine;

public class PreyChaseDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        PreyMemory memory = GetComponent<PreyMemory>();
        if (context == null || context.Transform == null || memory == null)
            return AIMoveIntent.None("prey missing");
        if (!memory.hasMemory || (memory.rememberedPrey == null && memory.rememberedPosition == Vector3.zero))
            return AIMoveIntent.None("prey no memory");

        Vector3 target = memory.rememberedPrey != null
            ? memory.rememberedPrey.transform.position
            : memory.rememberedPosition;

        Vector3 dir = target - context.Transform.position;
        dir.y = 0f;
        return new AIMoveIntent { direction = dir, weight = level, reason = "prey" };
    }
}
