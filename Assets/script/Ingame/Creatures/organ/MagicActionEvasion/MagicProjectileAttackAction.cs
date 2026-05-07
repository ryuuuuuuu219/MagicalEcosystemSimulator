using UnityEngine;

public class MagicProjectileAttackAction : MonoBehaviour, IAIAction
{
    public MagicElement element = MagicElement.Fire;
    public float spawnOffset = 1.5f;
    public float projectileScale = 0.45f;
    public MagicProjectileLaunchSettings projectileSettings = new MagicProjectileLaunchSettings(
        new Color(1f, 0.35f, 0.08f, 0.85f),
        55f,
        5f,
        3f,
        0f,
        0f,
        false,
        4f,
        0.2f);
    public MagicCircleLaunchSettings spellCircleSettings = MagicCircleLaunchSettings.Default;
    public float manaCost = 10f;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.Transform == null)
            return false;
        if (TryGetComponent<MagicCooldownState>(out var cooldown) && !cooldown.CanCast(context.Mana, manaCost))
            return false;

        Vector3 direction = ResolveDirection(context);
        MagicProjectileLaunchSettings settings = projectileSettings.WithFallbackEffectLifetime(4f);
        settings.magicManaCost = manaCost;
        GameObject projectile = MagicLaunchApi.LaunchImmediate(new MagicLaunchRequest
        {
            element = element,
            origin = context.Transform.position + Vector3.up * 0.7f,
            direction = direction,
            spawnOffset = spawnOffset,
            projectileScale = projectileScale,
            projectileSettings = settings,
            spellCircleSettings = spellCircleSettings,
            projectileName = "Organ Magic Projectile",
            casterResource = context.BodyResource
        });

        if (projectile == null)
            return false;

        if (cooldown != null)
            cooldown.MarkCast();
        return true;
    }

    Vector3 ResolveDirection(AIContext context)
    {
        if (TryGetComponent<PreyMemory>(out var preyMemory) && preyMemory.hasMemory)
        {
            Vector3 target = preyMemory.rememberedPrey != null
                ? preyMemory.rememberedPrey.transform.position
                : preyMemory.rememberedPosition;
            Vector3 toTarget = target - context.Transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        return context.Transform.forward;
    }
}
