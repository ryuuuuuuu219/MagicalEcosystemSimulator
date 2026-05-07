using UnityEngine;

public class RandomEvasionAction : MonoBehaviour, IAIAction
{
    public Vector3 evasionDirection;

    public bool TryAct(AIContext context, float deltaTime)
    {
        evasionDirection = Random.insideUnitSphere;
        evasionDirection.y = 0f;
        return false;
    }
}
