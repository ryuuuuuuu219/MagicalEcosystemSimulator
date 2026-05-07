public class MagicAttackAction : UnityEngine.MonoBehaviour, IAIAction
{
    public float manaCost = 10f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        MagicProjectileAttackAction projectileAttack = GetComponent<MagicProjectileAttackAction>();
        if (projectileAttack == null)
            projectileAttack = gameObject.AddComponent<MagicProjectileAttackAction>();

        projectileAttack.manaCost = manaCost;
        return projectileAttack.TryAct(context, deltaTime);
    }
}
