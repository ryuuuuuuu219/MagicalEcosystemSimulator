using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class OrganFoundationCheckpoint
{
    public string reason;
    public float time;
    public category phase;
    public float score;
    public int activeGeneCount;
    public int vestigialGeneCount;
    public List<string> installedOrgans = new();
    public List<string> vestigialOrgans = new();
    public List<AIComponentGene> genes = new();
    public GeneDataRecord geneData;
}

[System.Serializable]
public class OrganMutationEvent
{
    public string componentId;
    public string reason;
    public float time;
    public category phase;
    public bool enabled;
    public bool isVestigialOrgan;
    public float level;
    public float weight;
}

public class OrganFoundation : MonoBehaviour
{
    public bool runAnimalBrain = true;
    public bool enableRuntimeMutation = true;
    public float mutationInterval = 10f;
    public float runtimeMutationChanceScale = 1f;
    public int maxCheckpointCount = 32;
    public bool showInstalledOrganOverlay;
    public int overlayGeneLimit = 12;
    public Vector2 overlayPosition = new(24f, 120f);
    public Vector2 overlaySize = new(360f, 420f);
    public List<string> installedOrgans = new();
    public List<string> VestigialOrgans = new();
    public List<OrganFoundationCheckpoint> checkpoints = new();
    public List<OrganMutationEvent> mutationEvents = new();

    AnimalAIInstaller installer;
    AnimalBrain brain;
    Resource bodyResource;
    string overlayText;
    float mutationTimer;
    category lastSnapshotPhase;

    public bool IsBrainRunner => runAnimalBrain && isActiveAndEnabled && brain != null;

    void Awake()
    {
        RefreshReferences();
        RefreshInstalledOrganList();
        lastSnapshotPhase = bodyResource != null ? bodyResource.resourceCategory : category.herbivore;
        RecordCheckpoint("awake");
    }

    void Update()
    {
        RefreshReferences();
        DetectPhaseSnapshot();
        TickRuntimeMutation(Time.deltaTime);

        if (runAnimalBrain && brain != null)
            brain.TickBrain(Time.deltaTime);
    }

    public T EnsureOrgan<T>() where T : Component
    {
        RefreshReferences();
        if (installer == null)
            return null;

        AIComponentGene gene = installer.componentSet.EnsureGene(typeof(T).Name, true, IsVitalOrgan(typeof(T)));
        if (!gene.IsActive)
        {
            RefreshInstalledOrganList();
            return GetComponent<T>();
        }

        T component = installer.Ensure<T>();
        RefreshReferences();
        RefreshInstalledOrganList();
        RecordCheckpoint("ensure " + typeof(T).Name);
        return component;
    }

    public Component EnsureOrgan(System.Type componentType)
    {
        RefreshReferences();
        if (installer == null)
            return null;

        AIComponentGene gene = installer.componentSet.EnsureGene(componentType.Name, true, IsVitalOrgan(componentType));
        if (!gene.IsActive)
        {
            RefreshInstalledOrganList();
            return GetComponent(componentType);
        }

        Component component = installer.Ensure(componentType);
        RefreshReferences();
        RefreshInstalledOrganList();
        RecordCheckpoint("ensure " + componentType.Name);
        return component;
    }

    public void InstallComponentSet(AIComponentSet componentSet, string reason, bool preserveExistingGenes = false)
    {
        RefreshReferences();
        if (installer == null || componentSet == null)
            return;

        for (int i = 0; i < componentSet.genes.Count; i++)
        {
            if (preserveExistingGenes)
                installer.componentSet.ApplyPresetGene(componentSet.genes[i]);
            else
                installer.componentSet.SetGene(componentSet.genes[i]);
        }

        InstallActiveGenesFromSet();
        RefreshReferences();
        RefreshOrgansAfterChange(reason);
    }

    public bool IsOrganActive(Component component)
    {
        if (component == null || installer == null)
            return true;

        if (IsVitalOrgan(component.GetType()))
            return true;

        return installer.componentSet.IsActive(component.GetType().Name, true);
    }

    public void RecordCheckpoint(string reason)
    {
        RefreshReferences();
        RefreshInstalledOrganList();

        OrganFoundationCheckpoint checkpoint = new OrganFoundationCheckpoint
        {
            reason = reason,
            time = Time.time,
            phase = bodyResource != null ? bodyResource.resourceCategory : category.herbivore,
            installedOrgans = new List<string>(installedOrgans),
            vestigialOrgans = new List<string>(VestigialOrgans),
            genes = installer != null ? installer.componentSet.CloneGenes() : new List<AIComponentGene>()
        };
        checkpoint.geneData = GeneDataManager.RecordCheckpoint(gameObject, reason, checkpoint.time, checkpoint.genes);
        checkpoint.activeGeneCount = CountActiveGenes(checkpoint.genes);
        checkpoint.vestigialGeneCount = CountVestigialGenes(checkpoint.genes);
        checkpoints.Add(checkpoint);
        TrimCheckpoints();
    }

    [ContextMenu("Organ/Record Checkpoint")]
    public void RecordManualCheckpoint()
    {
        RecordCheckpoint("manual checkpoint");
    }

    [ContextMenu("Organ/Force Runtime Mutation Tick")]
    public void ForceRuntimeMutationTick()
    {
        float previousScale = runtimeMutationChanceScale;
        runtimeMutationChanceScale = Mathf.Max(1000f, runtimeMutationChanceScale);
        mutationTimer = Mathf.Max(0.1f, mutationInterval);
        TickRuntimeMutation(0f);
        runtimeMutationChanceScale = previousScale;
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
        if (bodyResource == null)
            bodyResource = GetComponent<Resource>();
    }

    void TickRuntimeMutation(float deltaTime)
    {
        if (!enableRuntimeMutation || installer == null || installer.componentSet == null)
            return;

        mutationTimer += deltaTime;
        if (mutationTimer < Mathf.Max(0.1f, mutationInterval))
            return;

        mutationTimer = 0f;
        bool changed = false;
        List<AIComponentGene> snapshot = installer.componentSet.CloneGenes();
        for (int i = 0; i < snapshot.Count; i++)
        {
            AIComponentGene gene = snapshot[i];
            if (!installer.componentSet.TryMutateRuntime(gene.componentId, runtimeMutationChanceScale, out AIComponentGene mutatedGene))
                continue;

            changed = true;
            RecordMutationEvent(mutatedGene, "runtime mutation");
            if (mutatedGene.isVestigialOrgan && !VestigialOrgans.Contains(mutatedGene.componentId))
            {
                VestigialOrgans.Add(mutatedGene.componentId);
                MarkUnusedDependenciesVestigial(mutatedGene.componentId);
            }
        }

        if (!changed)
            return;

        GeneDataManager.MutateRuntimeValues(gameObject, runtimeMutationChanceScale);
        RefreshOrgansAfterChange("runtime mutation");
    }

    void DetectPhaseSnapshot()
    {
        if (bodyResource == null)
            return;

        category current = bodyResource.resourceCategory;
        if (current == lastSnapshotPhase)
            return;

        lastSnapshotPhase = current;
        RecordCheckpoint("phase " + current);
    }

    void RefreshOrgansAfterChange(string reason)
    {
        InstallActiveGenesFromSet();
        RefreshInstalledOrganList();
        if (brain != null)
            brain.RefreshOrgans();
        if (installer != null && installer.componentSet != null)
            GeneDataManager.SetStructureGenes(installer.componentSet.CloneGenes());
        RecordCheckpoint(reason);
    }

    void InstallActiveGenesFromSet()
    {
        if (installer == null || installer.componentSet == null)
            return;

        List<AIComponentGene> genes = installer.componentSet.CloneGenes();
        for (int i = 0; i < genes.Count; i++)
        {
            AIComponentGene gene = genes[i];
            if (!gene.IsActive)
                continue;
            if (!OrganRelationLibrary.TryResolveOrganType(gene.componentId, out System.Type componentType))
                continue;
            if (GetComponent(componentType) == null)
                installer.Ensure(componentType);
        }
    }

    void MarkUnusedDependenciesVestigial(string disabledComponentId)
    {
        if (installer == null || installer.componentSet == null)
            return;

        List<string> unusedDependencies = OrganRelationLibrary.GetUnusedDependencyIdsAfterDisable(disabledComponentId, installer.componentSet);
        for (int i = 0; i < unusedDependencies.Count; i++)
        {
            string dependencyId = unusedDependencies[i];
            installer.componentSet.MarkVestigial(dependencyId);
            if (installer.componentSet.TryGetGene(dependencyId, out AIComponentGene dependencyGene))
                RecordMutationEvent(dependencyGene, "dependency vestigial after " + disabledComponentId);
            if (!VestigialOrgans.Contains(dependencyId))
                VestigialOrgans.Add(dependencyId);
        }
    }

    void RecordMutationEvent(AIComponentGene gene, string reason)
    {
        RefreshReferences();
        mutationEvents.Add(new OrganMutationEvent
        {
            componentId = gene.componentId,
            reason = reason,
            time = Time.time,
            phase = bodyResource != null ? bodyResource.resourceCategory : category.herbivore,
            enabled = gene.enabled,
            isVestigialOrgan = gene.isVestigialOrgan,
            level = gene.level,
            weight = gene.weight
        });

        int maxCount = Mathf.Max(1, maxCheckpointCount * 2);
        while (mutationEvents.Count > maxCount)
            mutationEvents.RemoveAt(0);
    }

    void TrimCheckpoints()
    {
        int maxCount = Mathf.Max(1, maxCheckpointCount);
        while (checkpoints.Count > maxCount)
            checkpoints.RemoveAt(0);
    }

    static int CountActiveGenes(List<AIComponentGene> genes)
    {
        if (genes == null)
            return 0;

        int count = 0;
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].IsActive)
                count++;
        }
        return count;
    }

    static int CountVestigialGenes(List<AIComponentGene> genes)
    {
        if (genes == null)
            return 0;

        int count = 0;
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].isVestigialOrgan)
                count++;
        }
        return count;
    }

    public static bool IsVitalOrgan(System.Type type)
    {
        return type == typeof(OrganFoundation)
            || type == typeof(AnimalBrain)
            || type == typeof(AnimalAIInstaller)
            || type == typeof(AIMemoryStore)
            || type == typeof(GroundMotor)
            || type == typeof(CreatureMotorBootstrap)
            || type == typeof(CreatureRelationResolver);
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
        builder.AppendLine($"checkpoints: {checkpoints.Count}");
        builder.AppendLine($"mutations: {mutationEvents.Count}");
        builder.AppendLine($"vestigial: {VestigialOrgans.Count}");
        if (checkpoints.Count > 0)
        {
            OrganFoundationCheckpoint latest = checkpoints[checkpoints.Count - 1];
            builder.AppendLine($"latest: {latest.reason} score:{latest.score:F2} active:{latest.activeGeneCount} vestigial:{latest.vestigialGeneCount}");
        }
        if (mutationEvents.Count > 0)
        {
            OrganMutationEvent latestMutation = mutationEvents[mutationEvents.Count - 1];
            builder.AppendLine($"last mutation: {latestMutation.componentId} {latestMutation.reason} lv:{latestMutation.level:F2} w:{latestMutation.weight:F2}");
        }

        AppendGeneOverlay(builder);
        overlayText = builder.ToString();
    }

    void AppendGeneOverlay(StringBuilder builder)
    {
        if (installer == null || installer.componentSet == null)
        {
            for (int i = 0; i < installedOrgans.Count; i++)
                builder.AppendLine("- " + installedOrgans[i]);
            return;
        }

        List<AIComponentGene> genes = installer.componentSet.CloneGenes();
        genes.Sort((a, b) => string.Compare(a.componentId, b.componentId, System.StringComparison.Ordinal));

        int limit = Mathf.Max(1, overlayGeneLimit);
        int shown = 0;
        for (int i = 0; i < genes.Count && shown < limit; i++)
        {
            AIComponentGene gene = genes[i];
            string state = gene.isVitalOrgan ? "VITAL" : gene.isVestigialOrgan ? "VEST" : gene.IsActive ? "ON" : "OFF";
            builder.AppendLine($"- {gene.componentId} [{state}] lv:{gene.level:F2} w:{gene.weight:F2} mt:{gene.mutationChanceT:F3} mg:{gene.mutationChanceG:F3}");
            shown++;
        }

        if (genes.Count > shown)
            builder.AppendLine($"... +{genes.Count - shown} genes");

        if (VestigialOrgans.Count > 0)
            builder.AppendLine("vestigial: " + string.Join(",", VestigialOrgans));
    }

    void OnGUI()
    {
        if (!showInstalledOrganOverlay || string.IsNullOrEmpty(overlayText))
            return;

        GUI.Box(new Rect(overlayPosition.x, overlayPosition.y, overlaySize.x, overlaySize.y), GUIContent.none);
        GUI.Label(new Rect(overlayPosition.x + 8f, overlayPosition.y + 8f, overlaySize.x - 16f, overlaySize.y - 16f), overlayText);
    }
}
