using System;
using UnityEngine;

public class AnimalAIInstaller : MonoBehaviour
{
    public AIComponentSet componentSet = new();

    public void InstallDefaultOrgans()
    {
        Ensure<AnimalBrain>();
        Ensure<AIMemoryStore>();
        Ensure<GroundMotor>();
        Ensure<CreatureMotorBootstrap>();
    }

    public T Ensure<T>() where T : Component
    {
        if (!TryGetComponent<T>(out var component))
            component = gameObject.AddComponent<T>();
        return component;
    }

    public Component Ensure(Type componentType)
    {
        if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            return null;

        Component existing = GetComponent(componentType);
        return existing != null ? existing : gameObject.AddComponent(componentType);
    }
}
