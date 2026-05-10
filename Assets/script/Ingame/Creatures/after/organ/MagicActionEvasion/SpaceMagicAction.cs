using UnityEngine;

public enum SpaceMagicMode
{
    Distortion,
    Teleport
}

public class SpaceMagicAction : MonoBehaviour, IAIAction
{
    public SpaceMagicMode mode;
    public bool hasAssignedMode;
    public float manaCost = 18f;
    public float spawnOffset = 1.5f;
    public float projectileScale = 0.6f;
    public MagicCircleLaunchSettings spellCircleSettings = MagicCircleLaunchSettings.Default;

    public static SpaceMagicAction Ensure(GameObject target)
    {
        if (target == null)
            return null;

        AnimalAIInstaller installer = target.GetComponent<AnimalAIInstaller>();
        SpaceMagicAction action = installer != null
            ? installer.Ensure<SpaceMagicAction>()
            : target.GetComponent<SpaceMagicAction>();
        if (action == null)
            action = target.AddComponent<SpaceMagicAction>();

        if (installer != null)
            installer.componentSet.ProtectGene(nameof(SpaceMagicAction), true);

        if (!action.hasAssignedMode)
        {
            action.mode = Random.value < 0.5f ? SpaceMagicMode.Distortion : SpaceMagicMode.Teleport;
            action.hasAssignedMode = true;
        }
        return action;
    }

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.Transform == null)
            return false;
        if (TryGetComponent<MagicCooldownState>(out var cooldown) && !cooldown.CanCast(context.Mana, manaCost))
            return false;

        Vector3 direction = ResolveDirection(context);
        MagicProjectileLaunchSettings settings = MagicProjectileLaunchSettings
            .ForElement(MagicElement.Space, 7f, mode == SpaceMagicMode.Distortion ? 5f : 4f)
            .WithFallbackEffectLifetime(mode == SpaceMagicMode.Distortion ? 5f : 4f);
        settings.magicManaCost = manaCost;

        GameObject projectile = MagicLaunchApi.LaunchImmediate(new MagicLaunchRequest
        {
            element = MagicElement.Space,
            origin = context.Transform.position + Vector3.up * 0.9f,
            direction = direction,
            spawnOffset = spawnOffset,
            projectileScale = projectileScale,
            projectileSettings = settings,
            spellCircleSettings = spellCircleSettings,
            projectileName = "Dominant Space Magic",
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
