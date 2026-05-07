public class MagicDefenseAction : UnityEngine.MonoBehaviour, IAIAction
{
    public float manaCost = 5f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        return false;
    }
}
