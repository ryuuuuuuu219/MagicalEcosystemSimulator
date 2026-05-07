using UnityEngine;

public static class OrganPresetLibrary
{
    public static void EnsureHerbivore(GameObject target)
    {
        AnimalAIInstaller installer = EnsureInstaller(target);
        if (installer == null)
            return;

        EnsureCore(installer);
        installer.Ensure<FoodMemory>();
        installer.Ensure<ThreatMemory>();
        installer.Ensure<FoodVisionSense>();
        installer.Ensure<PredatorVisionSense>();
        installer.Ensure<ThreatVisionSense>();
        installer.Ensure<FoodDesire>();
        installer.Ensure<ThreatAvoidanceDesire>();
        installer.Ensure<WanderDesire>();
        installer.Ensure<BoundaryAvoidanceDesire>();
        installer.Ensure<GrassEatAction>();
        installer.Ensure<CorpseEatAction>();
        installer.Ensure<FieldManaAbsorbAction>();
        installer.Ensure<ManaFieldSense>();
        RefreshBrain(target);
    }

    public static void EnsurePredator(GameObject target)
    {
        category phase = ResolveCategory(target, category.predator);
        EnsurePredator(target, phase);
    }

    public static void EnsurePredator(GameObject target, category phase)
    {
        AnimalAIInstaller installer = EnsureInstaller(target);
        if (installer == null)
            return;

        EnsureCore(installer);
        installer.Ensure<PreyMemory>();
        installer.Ensure<ThreatMemory>();
        installer.Ensure<PreyVisionSense>();
        installer.Ensure<ThreatVisionSense>();
        installer.Ensure<PreyChaseDesire>();
        installer.Ensure<ThreatAvoidanceDesire>();
        installer.Ensure<WanderDesire>();
        installer.Ensure<BoundaryAvoidanceDesire>();
        installer.Ensure<BiteAttackAction>();
        installer.Ensure<MeleeAttackAction>();
        installer.Ensure<ThreatPulseEmitter>();
        installer.Ensure<FieldManaAbsorbAction>();
        installer.Ensure<ManaFieldSense>();
        installer.Ensure<PredatorPhaseEvolutionAction>();

        if (GetPhaseRank(phase) >= GetPhaseRank(category.highpredator))
        {
            installer.Ensure<ChargeAttackAction>();
            installer.Ensure<MagicAttackAction>();
            installer.Ensure<MagicProjectileAttackAction>();
            installer.Ensure<MagicCooldownState>();
        }

        RefreshBrain(target);
    }

    public static void EnsureForCurrentCategory(GameObject target)
    {
        category value = ResolveCategory(target, category.herbivore);
        if (GetPhaseRank(value) >= GetPhaseRank(category.predator))
            EnsurePredator(target, value);
        else
            EnsureHerbivore(target);
    }

    static void EnsureCore(AnimalAIInstaller installer)
    {
        installer.InstallDefaultOrgans();
        installer.Ensure<CreatureRelationResolver>();
    }

    static AnimalAIInstaller EnsureInstaller(GameObject target)
    {
        if (target == null)
            return null;
        if (!target.TryGetComponent<AnimalAIInstaller>(out var installer))
            installer = target.AddComponent<AnimalAIInstaller>();
        return installer;
    }

    static category ResolveCategory(GameObject target, category fallback)
    {
        if (target != null && target.TryGetComponent<Resource>(out var resource))
            return resource.resourceCategory;
        return fallback;
    }

    static int GetPhaseRank(category value)
    {
        switch (value)
        {
            case category.grass:
                return 1;
            case category.herbivore:
                return 2;
            case category.predator:
                return 3;
            case category.highpredator:
                return 4;
            case category.dominant:
                return 5;
            default:
                return 0;
        }
    }

    static void RefreshBrain(GameObject target)
    {
        if (target != null && target.TryGetComponent<AnimalBrain>(out var brain))
            brain.RefreshOrgans();
    }
}
