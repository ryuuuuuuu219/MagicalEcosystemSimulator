using System;
using UnityEngine;

public class AnimalAIInstaller : MonoBehaviour
{
    public AIComponentSet componentSet = new();

    public void InstallDefaultOrgans()
    {
        Ensure<OrganFoundation>();
        Ensure<AnimalBrain>();
        Ensure<AIMemoryStore>();
        Ensure<GroundMotor>();
        Ensure<CreatureMotorBootstrap>();
    }

    public T Ensure<T>() where T : Component
    {
        return Ensure(typeof(T)) as T;
    }

    public Component Ensure(Type componentType)
    {
        if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            return null;

        componentSet.EnsureGene(componentType.Name, true, OrganFoundation.IsVitalOrgan(componentType));
        Component component = EnsureWithoutRelations(componentType);
        OrganRelationLibrary.EnsureDependencies(this, componentType);
        return component;
    }

    public Component EnsureWithoutRelations(Type componentType)
    {
        if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            return null;

        componentSet.EnsureGene(componentType.Name, true, OrganFoundation.IsVitalOrgan(componentType));
        Component existing = GetComponent(componentType);
        return existing != null ? existing : gameObject.AddComponent(componentType);
    }
}
