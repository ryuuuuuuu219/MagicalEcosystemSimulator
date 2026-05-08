using System;
using System.Collections.Generic;
using UnityEngine;

public static class OrganRelationLibrary
{
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

    public static void EnsureDependencies(AnimalAIInstaller installer, Type organType)
    {
        if (installer == null || organType == null)
            return;

        EnsureDependencies(installer, organType, new HashSet<Type>());
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
