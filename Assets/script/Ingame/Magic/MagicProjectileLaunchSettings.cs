using UnityEngine;

[System.Serializable]
public struct MagicProjectileLaunchSettings
{
    public Color projectileColor;
    public float projectileSpeed;
    public float projectileLifetime;
    public float effectRadius;
    public float iceSpikeHeight;
    public float iceSpikeRadius;
    public bool wrapNonTerrainTargets;
    public float effectLifetime;
    public float envelopeLifetime;
    public float envelopePadding;
    public float magicDamage;
    public float magicManaCost;
    public float magicDamageToManaRate;
    public float magicRecoveryWindow;
    public int magicTargetCount;
    public float magicMaxNetGainPerCast;
    public float phaseMagicCostMultiplier;

    public MagicProjectileLaunchSettings(
        Color projectileColor,
        float projectileSpeed,
        float projectileLifetime,
        float effectRadius,
        float iceSpikeHeight,
        float iceSpikeRadius,
        bool wrapNonTerrainTargets,
        float effectLifetime,
        float envelopePadding)
    {
        this.projectileColor = projectileColor;
        this.projectileSpeed = projectileSpeed;
        this.projectileLifetime = projectileLifetime;
        this.effectRadius = effectRadius;
        this.iceSpikeHeight = iceSpikeHeight;
        this.iceSpikeRadius = iceSpikeRadius;
        this.wrapNonTerrainTargets = wrapNonTerrainTargets;
        this.effectLifetime = effectLifetime;
        this.envelopeLifetime = effectLifetime;
        this.envelopePadding = envelopePadding;
        this.magicDamage = 12f;
        this.magicManaCost = 8f;
        this.magicDamageToManaRate = 1f;
        this.magicRecoveryWindow = projectileLifetime + effectLifetime;
        this.magicTargetCount = 1;
        this.magicMaxNetGainPerCast = 0f;
        this.phaseMagicCostMultiplier = 1f;
    }

    public MagicProjectileLaunchSettings WithFallbackEffectLifetime(float fallbackEffectLifetime)
    {
        if (effectLifetime > 0f)
            return this;

        MagicProjectileLaunchSettings settings = this;
        settings.effectLifetime = envelopeLifetime > 0f ? envelopeLifetime : fallbackEffectLifetime;
        if (settings.envelopeLifetime <= 0f)
            settings.envelopeLifetime = settings.effectLifetime;
        if (settings.magicDamage <= 0f)
            settings.magicDamage = 12f;
        if (settings.magicManaCost <= 0f)
            settings.magicManaCost = 8f;
        if (settings.magicDamageToManaRate <= 0f)
            settings.magicDamageToManaRate = 1f;
        if (settings.magicRecoveryWindow <= 0f)
            settings.magicRecoveryWindow = settings.projectileLifetime + settings.effectLifetime;
        if (settings.magicTargetCount <= 0)
            settings.magicTargetCount = 1;
        if (settings.phaseMagicCostMultiplier <= 0f)
            settings.phaseMagicCostMultiplier = 1f;
        return settings;
    }

    public static MagicProjectileLaunchSettings ForElement(
        MagicElement element,
        float defaultProjectileLifetime,
        float defaultEffectLifetime)
    {
        switch (element)
        {
            case MagicElement.Fire:
                return new MagicProjectileLaunchSettings(new Color(1f, 0.35f, 0.08f, 0.85f), 55f, defaultProjectileLifetime, 3f, 0f, 0f, false, defaultEffectLifetime, 0.2f);
            case MagicElement.Ice:
                return new MagicProjectileLaunchSettings(new Color(0.55f, 0.9f, 1f, 0.8f), 60f, defaultProjectileLifetime, 2f, 3f, 0.6f, true, defaultEffectLifetime, 0.25f);
            case MagicElement.Lightning:
                return new MagicProjectileLaunchSettings(new Color(1f, 0.95f, 0.25f, 0.9f), 120f, 4f, 1.5f, 0f, 0f, false, 3f, 0.15f);
            case MagicElement.Wind:
                return new MagicProjectileLaunchSettings(new Color(0.65f, 1f, 0.75f, 0.45f), 75f, 6f, 4f, 0f, 0f, false, 4f, 0.3f);
            case MagicElement.Space:
                return new MagicProjectileLaunchSettings(new Color(0.75f, 0.45f, 1f, 0.7f), 50f, 7f, 2.5f, 0f, 0f, false, 5f, 0.25f);
            default:
                return new MagicProjectileLaunchSettings(new Color(0.55f, 0.9f, 1f, 0.8f), 60f, defaultProjectileLifetime, 2f, 3f, 0.6f, true, defaultEffectLifetime, 0.25f);
        }
    }
}
