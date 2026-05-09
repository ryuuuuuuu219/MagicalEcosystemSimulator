using UnityEngine;

public static class OrganPresetLibrary
{
    public static void EnsureHerbivore(GameObject target)
    {
        AnimalAIInstaller installer = EnsureInstaller(target);
        if (installer == null)
            return;

        OrganFoundation foundation = EnsureCore(installer);
        foundation.InstallComponentSet(CreateHerbivorePreset(), "herbivore preset", true);
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
        foundation.InstallComponentSet(CreatePredatorPreset(phase), phase + " preset", true);
        RefreshBrain(target);
    }

    public static AIComponentSet CreateHerbivorePreset()
    {
        AIComponentSet set = CreateCorePreset();
        AddActive<FoodMemory>(set);
        AddActive<ThreatMemory>(set);
        AddActive<FoodVisionSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<PredatorVisionSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ThreatVisionSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<FoodDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ThreatAvoidanceDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<WanderDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<BoundaryAvoidanceDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<GrassEatAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<CorpseEatAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<FieldManaAbsorbAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ManaFieldSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);

        AddOptional<ManaFieldAttractionDesire>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<DamageAvoidanceDesire>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<RandomEvasionAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<MagicAttackAction>(set, mutationChanceT: 0.0001f, mutationChanceG: 0.003f);
        AddOptional<MagicProjectileAttackAction>(set, mutationChanceT: 0.0001f, mutationChanceG: 0.003f);
        AddOptional<MagicCooldownState>(set, mutationChanceT: 0.0001f, mutationChanceG: 0.003f);
        return set;
    }

    public static AIComponentSet CreatePredatorPreset(category phase)
    {
        AIComponentSet set = CreateCorePreset();
        AddActive<PreyMemory>(set);
        AddActive<ThreatMemory>(set);
        AddActive<PreyVisionSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ThreatVisionSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<PreyChaseDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ThreatAvoidanceDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<WanderDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<BoundaryAvoidanceDesire>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<BiteAttackAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<MeleeAttackAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ThreatPulseEmitter>(set);
        AddActive<FieldManaAbsorbAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<ManaFieldSense>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
        AddActive<PredatorPhaseEvolutionAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);

        if (GetPhaseRank(phase) >= GetPhaseRank(category.highpredator))
        {
            AddActive<ChargeAttackAction>(set, mutationChanceT: 0.001f, mutationChanceG: 0.02f);
            AddActive<MagicAttackAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
            AddActive<MagicProjectileAttackAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
            AddActive<MagicCooldownState>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        }
        else
        {
            AddOptional<ChargeAttackAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
            AddOptional<MagicAttackAction>(set, mutationChanceT: 0.0002f, mutationChanceG: 0.006f);
            AddOptional<MagicProjectileAttackAction>(set, mutationChanceT: 0.0002f, mutationChanceG: 0.006f);
            AddOptional<MagicCooldownState>(set, mutationChanceT: 0.0002f, mutationChanceG: 0.006f);
        }

        AddOptional<ManaFieldAttractionDesire>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<DamageAvoidanceDesire>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<CounterAttackAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<RandomEvasionAction>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        AddOptional<ProportionalNavigationSteering>(set, mutationChanceT: 0.0005f, mutationChanceG: 0.01f);
        return set;
    }

    static AIComponentSet CreateCorePreset()
    {
        AIComponentSet set = new AIComponentSet();
        AddVital<OrganFoundation>(set);
        AddVital<AnimalAIInstaller>(set);
        AddVital<AnimalBrain>(set);
        AddVital<AIMemoryStore>(set);
        AddVital<GroundMotor>(set);
        AddVital<CreatureMotorBootstrap>(set);
        AddVital<CreatureRelationResolver>(set);
        return set;
    }

    static void AddVital<T>(AIComponentSet set) where T : Component
    {
        AIComponentGene gene = AIComponentGene.CreateDefault(typeof(T).Name, true, true);
        gene.mutationChanceT = 0f;
        gene.mutationChanceG = 0f;
        set.SetGene(gene);
    }

    static void AddActive<T>(AIComponentSet set, float mutationChanceT = 0f, float mutationChanceG = 0f) where T : Component
    {
        AIComponentGene gene = AIComponentGene.CreateDefault(typeof(T).Name, true, false);
        gene.mutationChanceT = mutationChanceT;
        gene.mutationChanceG = mutationChanceG;
        set.SetGene(gene);
    }

    static void AddOptional<T>(AIComponentSet set, float mutationChanceT, float mutationChanceG) where T : Component
    {
        AIComponentGene gene = AIComponentGene.CreateDefault(typeof(T).Name, false, false);
        gene.mutationChanceT = mutationChanceT;
        gene.mutationChanceG = mutationChanceG;
        set.SetGene(gene);
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
