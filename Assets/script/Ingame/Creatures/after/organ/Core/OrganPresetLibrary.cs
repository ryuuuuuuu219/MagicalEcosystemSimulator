using UnityEngine;

public static class OrganPresetLibrary
{
    public static void EnsureHerbivore(GameObject target)
    {
        AnimalAIInstaller installer = EnsureInstaller(target);
        if (installer == null)
            return;

        OrganFoundation foundation = EnsureCore(installer);
        foundation.EnsureOrgan<FoodMemory>();
        foundation.EnsureOrgan<ThreatMemory>();
        foundation.EnsureOrgan<FoodVisionSense>();
        foundation.EnsureOrgan<PredatorVisionSense>();
        foundation.EnsureOrgan<ThreatVisionSense>();
        foundation.EnsureOrgan<FoodDesire>();
        foundation.EnsureOrgan<ThreatAvoidanceDesire>();
        foundation.EnsureOrgan<WanderDesire>();
        foundation.EnsureOrgan<BoundaryAvoidanceDesire>();
        foundation.EnsureOrgan<GrassEatAction>();
        foundation.EnsureOrgan<CorpseEatAction>();
        foundation.EnsureOrgan<FieldManaAbsorbAction>();
        foundation.EnsureOrgan<ManaFieldSense>();
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

        OrganFoundation foundation = EnsureCore(installer);
        foundation.EnsureOrgan<PreyMemory>();
        foundation.EnsureOrgan<ThreatMemory>();
        foundation.EnsureOrgan<PreyVisionSense>();
        foundation.EnsureOrgan<ThreatVisionSense>();
        foundation.EnsureOrgan<PreyChaseDesire>();
        foundation.EnsureOrgan<ThreatAvoidanceDesire>();
        foundation.EnsureOrgan<WanderDesire>();
        foundation.EnsureOrgan<BoundaryAvoidanceDesire>();
        foundation.EnsureOrgan<BiteAttackAction>();
        foundation.EnsureOrgan<MeleeAttackAction>();
        foundation.EnsureOrgan<ThreatPulseEmitter>();
        foundation.EnsureOrgan<FieldManaAbsorbAction>();
        foundation.EnsureOrgan<ManaFieldSense>();
        foundation.EnsureOrgan<PredatorPhaseEvolutionAction>();

        if (GetPhaseRank(phase) >= GetPhaseRank(category.highpredator))
        {
            foundation.EnsureOrgan<ChargeAttackAction>();
            foundation.EnsureOrgan<MagicAttackAction>();
            foundation.EnsureOrgan<MagicProjectileAttackAction>();
            foundation.EnsureOrgan<MagicCooldownState>();
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

    static OrganFoundation EnsureCore(AnimalAIInstaller installer)
    {
        installer.InstallDefaultOrgans();
        installer.Ensure<CreatureRelationResolver>();
        OrganFoundation foundation = installer.Ensure<OrganFoundation>();
        foundation.RefreshInstalledOrganList();
        return foundation;
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
