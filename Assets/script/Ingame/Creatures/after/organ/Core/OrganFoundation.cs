using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class OrganFoundation : MonoBehaviour
{
    public bool runAnimalBrain = true;
    public bool showInstalledOrganOverlay;
    public Vector2 overlayPosition = new(24f, 120f);
    public Vector2 overlaySize = new(360f, 420f);
    public List<string> installedOrgans = new();

    AnimalAIInstaller installer;
    AnimalBrain brain;
    string overlayText;

    public bool IsBrainRunner => runAnimalBrain && isActiveAndEnabled && brain != null;

    void Awake()
    {
        RefreshReferences();
        RefreshInstalledOrganList();
    }

    void Update()
    {
        RefreshReferences();

        if (runAnimalBrain && brain != null)
            brain.TickBrain(Time.deltaTime);
    }

    public T EnsureOrgan<T>() where T : Component
    {
        RefreshReferences();
        if (installer == null)
            return null;

        T component = installer.Ensure<T>();
        RefreshReferences();
        RefreshInstalledOrganList();
        return component;
    }

    public Component EnsureOrgan(System.Type componentType)
    {
        RefreshReferences();
        if (installer == null)
            return null;

        Component component = installer.Ensure(componentType);
        RefreshReferences();
        RefreshInstalledOrganList();
        return component;
    }

    public void RefreshInstalledOrganList()
    {
        installedOrgans.Clear();

        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
                continue;

            string typeName = behaviour.GetType().Name;
            if (IsOrganComponent(behaviour))
                installedOrgans.Add(typeName);
        }

        installedOrgans.Sort();
        RebuildOverlayText();
    }

    void RefreshReferences()
    {
        if (installer == null)
            installer = GetComponent<AnimalAIInstaller>();
        if (brain == null)
            brain = GetComponent<AnimalBrain>();
    }

    static bool IsOrganComponent(MonoBehaviour behaviour)
    {
        return behaviour is OrganFoundation
            || behaviour is AnimalAIInstaller
            || behaviour is AnimalBrain
            || behaviour is IAISense
            || behaviour is IAIDesire
            || behaviour is IAISteering
            || behaviour is IAIAction
            || behaviour is AIMemoryStore
            || behaviour is FoodMemory
            || behaviour is PreyMemory
            || behaviour is ThreatMemory
            || behaviour is TargetTracker
            || behaviour is GroundMotor
            || behaviour is CreatureMotorBootstrap
            || behaviour is CreatureRelationResolver;
    }

    void RebuildOverlayText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"{name} organs ({installedOrgans.Count})");
        builder.AppendLine(runAnimalBrain ? "runner: AnimalBrain" : "runner: legacy behaviour");
        for (int i = 0; i < installedOrgans.Count; i++)
            builder.AppendLine("- " + installedOrgans[i]);
        overlayText = builder.ToString();
    }

    void OnGUI()
    {
        if (!showInstalledOrganOverlay || string.IsNullOrEmpty(overlayText))
            return;

        GUI.Box(new Rect(overlayPosition.x, overlayPosition.y, overlaySize.x, overlaySize.y), GUIContent.none);
        GUI.Label(new Rect(overlayPosition.x + 8f, overlayPosition.y + 8f, overlaySize.x - 16f, overlaySize.y - 16f), overlayText);
    }
}
