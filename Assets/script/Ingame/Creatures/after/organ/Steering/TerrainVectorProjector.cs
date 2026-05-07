using UnityEngine;

public class TerrainVectorProjector : MonoBehaviour, IAISteering
{
    public Vector3 Steer(AIContext context, Vector3 desiredVector)
    {
        if (context == null || context.Transform == null)
            return desiredVector;

        return AnimalAICommon.AdjustMovementVectorForTerrain(context.Terrain, context.Transform.position, desiredVector);
    }
}
