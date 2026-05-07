using UnityEngine;

public class PreyChaseDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        PreyMemory memory = GetComponent<PreyMemory>();
        if (context == null || context.Transform == null || memory == null)
            return AIMoveIntent.None("prey missing");

        Vector3 target = memory.rememberedPrey != null
            ? memory.rememberedPrey.transform.position
            : memory.rememberedPosition;

        Vector3 dir = target - context.Transform.position;
        dir.y = 0f;
        return new AIMoveIntent { direction = dir, weight = level, reason = "prey" };
    }
}
