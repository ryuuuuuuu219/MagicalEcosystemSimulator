using UnityEngine;

public class BoundaryAvoidanceDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        if (context == null || context.Transform == null)
            return AIMoveIntent.None("boundary missing");

        Vector3 dir = AnimalAICommon.ComputeBoundaryVector(context.Terrain, context.Transform.position);
        return new AIMoveIntent { direction = dir, weight = level, reason = "boundary" };
    }
}
