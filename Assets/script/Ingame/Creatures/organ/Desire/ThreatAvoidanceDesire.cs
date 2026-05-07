using UnityEngine;

public class ThreatAvoidanceDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        ThreatMemory memory = GetComponent<ThreatMemory>();
        if (context == null || context.Transform == null || memory == null)
            return AIMoveIntent.None("threat missing");

        Vector3 threatPosition = memory.rememberedThreat != null
            ? memory.rememberedThreat.transform.position
            : memory.rememberedPosition;

        Vector3 dir = context.Transform.position - threatPosition;
        dir.y = 0f;
        return new AIMoveIntent { direction = dir, weight = level, reason = "threat avoid" };
    }
}
