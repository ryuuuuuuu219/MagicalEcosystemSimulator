using UnityEngine;

public class CreatureDeathState : MonoBehaviour, IAIAction
{
    public bool IsDead(AIContext context)
    {
        return context != null && context.Health <= 0f;
    }

    public bool TryAct(AIContext context, float deltaTime)
    {
        return IsDead(context);
    }
}
