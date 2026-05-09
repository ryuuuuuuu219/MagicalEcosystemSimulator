using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class OrganRelationLibrary
{
    static readonly Dictionary<string, Type> TypeCache = new();

    static readonly Dictionary<Type, Type[]> DependencyWhitelist = new()
    {
        { typeof(AnimalBrain), new[] { typeof(AIMemoryStore), typeof(GroundMotor), typeof(CreatureMotorBootstrap) } },
        { typeof(GroundMotor), new[] { typeof(CreatureMotorBootstrap) } },
        { typeof(FoodVisionSense), new[] { typeof(FoodMemory) } },
        { typeof(PredatorVisionSense), new[] { typeof(ThreatMemory) } },
        { typeof(PreyVisionSense), new[] { typeof(PreyMemory), typeof(TargetTracker), typeof(CreatureRelationResolver) } },
        { typeof(ThreatVisionSense), new[] { typeof(ThreatMemory) } },
        { typeof(FoodDesire), new[] { typeof(FoodMemory) } },
        { typeof(PreyChaseDesire), new[] { typeof(PreyMemory) } },
        { typeof(ThreatAvoidanceDesire), new[] { typeof(ThreatMemory) } },
        { typeof(BiteAttackAction), new[] { typeof(PreyMemory), typeof(ThreatPulseEmitter) } },
        { typeof(MeleeAttackAction), new[] { typeof(PreyMemory), typeof(ThreatPulseEmitter) } },
        { typeof(ChargeAttackAction), new[] { typeof(PreyMemory), typeof(ThreatPulseEmitter), typeof(GroundMotor) } },
        { typeof(CorpseEatAction), new[] { typeof(FoodMemory) } },
        { typeof(MagicAttackAction), new[] { typeof(MagicProjectileAttackAction), typeof(MagicCooldownState) } },
        { typeof(MagicProjectileAttackAction), new[] { typeof(PreyMemory), typeof(MagicCooldownState) } },
        { typeof(MagicEvasionAction), new[] { typeof(MagicCooldownState) } },
        { typeof(MagicDefenseAction), new[] { typeof(MagicCooldownState) } },
        { typeof(PredatorPhaseEvolutionAction), new[] { typeof(ManaFieldSense) } },
        { typeof(FieldManaAbsorbAction), new[] { typeof(ManaFieldSense) } },
        { typeof(ManaFieldAttractionDesire), new[] { typeof(ManaFieldSense) } },
        { typeof(ProportionalNavigationSteering), new[] { typeof(TargetTracker) } },
    };

    public static bool TryGetDependencies(Type organType, out Type[] dependencies)
    {
        return DependencyWhitelist.TryGetValue(organType, out dependencies);
    }

    public static bool TryResolveOrganType(string componentId, out Type componentType)
    {
        if (string.IsNullOrEmpty(componentId))
        {
            componentType = null;
            return false;
        }

        if (TypeCache.TryGetValue(componentId, out componentType))
            return componentType != null;

        Assembly assembly = typeof(AnimalBrain).Assembly;
        Type[] types = assembly.GetTypes();
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            if (type.Name != componentId || !typeof(Component).IsAssignableFrom(type))
                continue;

            TypeCache[componentId] = type;
            componentType = type;
            return true;
        }

        TypeCache[componentId] = null;
        componentType = null;
        return false;
    }

    public static void EnsureDependencies(AnimalAIInstaller installer, Type organType)
    {
        if (installer == null || organType == null)
            return;

        EnsureDependencies(installer, organType, new HashSet<Type>());
    }

    public static List<string> GetUnusedDependencyIdsAfterDisable(string disabledComponentId, AIComponentSet componentSet)
    {
        List<string> unused = new List<string>();
        if (componentSet == null || !TryResolveOrganType(disabledComponentId, out Type disabledType))
            return unused;
        if (!TryGetDependencies(disabledType, out Type[] dependencies))
            return unused;

        for (int i = 0; i < dependencies.Length; i++)
        {
            Type dependency = dependencies[i];
            if (dependency == null || OrganFoundation.IsVitalOrgan(dependency))
                continue;

            if (!IsDependencyUsedByAnyActiveParent(dependency, disabledType, componentSet))
                unused.Add(dependency.Name);
        }

        return unused;
    }

    static bool IsDependencyUsedByAnyActiveParent(Type dependency, Type disabledParent, AIComponentSet componentSet)
    {
        List<AIComponentGene> genes = componentSet.CloneGenes();
        for (int i = 0; i < genes.Count; i++)
        {
            AIComponentGene gene = genes[i];
            if (!gene.IsActive)
                continue;
            if (!TryResolveOrganType(gene.componentId, out Type parentType))
                continue;
            if (parentType == disabledParent)
                continue;
            if (!TryGetDependencies(parentType, out Type[] parentDependencies))
                continue;

            for (int d = 0; d < parentDependencies.Length; d++)
            {
                if (parentDependencies[d] == dependency)
                    return true;
            }
        }

        return false;
    }

    static void EnsureDependencies(AnimalAIInstaller installer, Type organType, HashSet<Type> visited)
    {
        if (!visited.Add(organType))
            return;

        if (!TryGetDependencies(organType, out Type[] dependencies))
            return;

        for (int i = 0; i < dependencies.Length; i++)
        {
            Type dependency = dependencies[i];
            if (dependency == null || !typeof(Component).IsAssignableFrom(dependency))
                continue;

            installer.EnsureWithoutRelations(dependency);
            EnsureDependencies(installer, dependency, visited);
        }
    }
}
