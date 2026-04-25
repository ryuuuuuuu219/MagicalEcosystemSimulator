using UnityEngine;

public partial class WorldUIManager
{
    [Header("Pending UI Property Sources")]
    [SerializeField] WorldGenerator worldGenerator;
    [SerializeField] ResourceDispenser resourceDispenser;
    [SerializeField] FreeFlyCamera freeFlyCamera;
    [SerializeField] PerformanceBudgetMonitor performanceBudgetMonitor;

    void EnsurePendingUiPropertySources()
    {
        if (worldGenerator == null)
            worldGenerator = FindFirstObjectByType<WorldGenerator>();

        if (resourceDispenser == null)
        {
            if (grassManager != null)
                resourceDispenser = grassManager.GetComponent<ResourceDispenser>();

            if (resourceDispenser == null)
                resourceDispenser = FindFirstObjectByType<ResourceDispenser>();
        }

        if (freeFlyCamera == null)
        {
            if (mainCamera != null)
                freeFlyCamera = mainCamera.GetComponent<FreeFlyCamera>();

            if (freeFlyCamera == null)
                freeFlyCamera = FindFirstObjectByType<FreeFlyCamera>();
        }

        if (generationController == null)
            generationController = FindFirstObjectByType<AdvanceGenerationController>();

        if (virtualGaugeManager == null)
            virtualGaugeManager = VirtualGaugeManager.Instance != null
                ? VirtualGaugeManager.Instance
                : FindFirstObjectByType<VirtualGaugeManager>();

        if (performanceBudgetMonitor == null)
            performanceBudgetMonitor = FindFirstObjectByType<PerformanceBudgetMonitor>();
    }

    public WorldGenerator TerrainPropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return worldGenerator;
        }
    }

    public ResourceDispenser SpawnPropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return resourceDispenser;
        }
    }

    public ResourceDispenser EnergyPropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return resourceDispenser;
        }
    }

    public ResourceDispenser GrassSpawnPropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return resourceDispenser;
        }
    }

    public AdvanceGenerationController GenerationPropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return generationController;
        }
    }

    public VirtualGaugeManager VirtualGaugePropertiesSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return virtualGaugeManager;
        }
    }

    public FreeFlyCamera CameraPresetSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return freeFlyCamera;
        }
    }

    public PerformanceBudgetMonitor PerformanceMonitorSource
    {
        get
        {
            EnsurePendingUiPropertySources();
            return performanceBudgetMonitor;
        }
    }

    public LayerMask SelectableLayerSource => selectableLayer;

    static GameObject GetRandomAliveObject(System.Collections.Generic.List<GameObject> list)
    {
        if (list == null || list.Count == 0)
            return null;

        int start = Random.Range(0, list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            GameObject candidate = list[(start + i) % list.Count];
            if (candidate != null)
                return candidate;
        }

        return null;
    }
}
