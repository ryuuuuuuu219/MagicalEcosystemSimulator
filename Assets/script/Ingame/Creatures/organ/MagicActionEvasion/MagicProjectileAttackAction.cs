public class MagicProjectileAttackAction : UnityEngine.MonoBehaviour, IAIAction
{
    public float manaCost = 10f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        return false;
    }
}
