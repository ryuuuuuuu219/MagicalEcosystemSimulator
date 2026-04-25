using UnityEngine;

public partial class WorldUIManager
{
    public float GetTimeScale()
    {
        return Time.timeScale;
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = Mathf.Max(0f, scale);
    }

    public void SetEnergyConsumptionParameters(
        float decompose,
        float carbonToEnergy,
        float metabolicEnergy,
        float metabolicHeat,
        float idleCost,
        float moveCost,
        float accelerationCost,
        float brakingCost,
        float turnCost,
        float decompositionHeat)
    {
        ResourceDispenser source = EnergyPropertiesSource;
        if (source == null)
            return;

        source.decomposeRate = Mathf.Max(0f, decompose);
        source.carbonToEnergyRate = Mathf.Max(0f, carbonToEnergy);
        source.metabolicEnergyPerCarbon = Mathf.Max(0f, metabolicEnergy);
        source.metabolicHeatPerCarbon = Mathf.Max(0f, metabolicHeat);
        source.idleEnergyCostPerSec = Mathf.Max(0f, idleCost);
        source.moveEnergyCostPerSec = Mathf.Max(0f, moveCost);
        source.accelerationEnergyCostPerUnit = Mathf.Max(0f, accelerationCost);
        source.brakingEnergyCostPerUnit = Mathf.Max(0f, brakingCost);
        source.turnEnergyCostPerDegree = Mathf.Max(0f, turnCost);
        source.decompositionHeatPerCarbon = Mathf.Max(0f, decompositionHeat);
    }

    public void SetTerrainParameters(
        int terrainSizeValue,
        int heightmapResolutionValue,
        float heightScaleValue,
        float noiseScaleValue,
        int octavesValue,
        float persistenceValue,
        float lacunarityValue,
        float waterHeightValue)
    {
        WorldGenerator source = TerrainPropertiesSource;
        if (source == null)
            return;

        source.terrainSize = Mathf.Max(1, terrainSizeValue);
        source.heightmapResolution = Mathf.Max(33, heightmapResolutionValue);
        source.heightScale = Mathf.Max(0f, heightScaleValue);
        source.noiseScale = Mathf.Max(0.0001f, noiseScaleValue);
        source.octaves = Mathf.Max(1, octavesValue);
        source.persistence = Mathf.Clamp01(persistenceValue);
        source.lacunarity = Mathf.Max(1f, lacunarityValue);
        source.waterHeight = waterHeightValue;
    }

    public void SetInitialSpawnCounts(int grassCount, int herbivoreCount, int predatorCount)
    {
        ResourceDispenser source = SpawnPropertiesSource;
        if (source == null)
            return;

        source.initialGrassCount = Mathf.Max(0, grassCount);
        source.initialHerbivoreCount = Mathf.Max(0, herbivoreCount);
        source.initialPredatorCount = Mathf.Max(0, predatorCount);
    }

    public void SetGenerationSpawnCounts(int grassCount, int herbivoreCount, int predatorCount)
    {
        ResourceDispenser source = SpawnPropertiesSource;
        if (source == null)
            return;

        source.grassCountPerGeneration = Mathf.Max(0, grassCount);
        source.herbivoreCountPerGeneration = Mathf.Max(0, herbivoreCount);
        source.predatorCountPerGeneration = Mathf.Max(0, predatorCount);
    }

    public void SetVegetationSpawnParameters(float radiusValue, float densityValue, int plantCountValue, float maxSlopeValue)
    {
        ResourceDispenser source = GrassSpawnPropertiesSource;
        if (source == null)
            return;

        source.radius = Mathf.Max(0.1f, radiusValue);
        source.density = Mathf.Max(0f, densityValue);
        source.plantCount = Mathf.Max(0, plantCountValue);
        source.maxSlope = Mathf.Clamp(maxSlopeValue, 0f, 90f);
    }

    public void SetSelectableLayer(LayerMask layerMask)
    {
        selectableLayer = layerMask;
    }

    public void SetVirtualGaugeVisibility(bool visible, bool showHerbivores, bool showPredators)
    {
        VirtualGaugeManager source = VirtualGaugePropertiesSource;
        if (source == null)
            return;

        source.SetGaugeVisibility(visible, showHerbivores, showPredators);
    }

    public void SetVirtualGaugeDisplaySettings(bool showHealth, bool showEnergy, bool showCarbon, float alpha)
    {
        VirtualGaugeManager source = VirtualGaugePropertiesSource;
        if (source == null)
            return;

        source.SetGaugeDisplayOptions(showHealth, showEnergy, showCarbon, alpha);
    }

    public void SetVirtualGaugeStatusSelection(bool showHealth, bool showCarbon, bool showEnergy)
    {
        SetVirtualGaugeDisplaySettings(showHealth, showEnergy, showCarbon, GetVirtualGaugeAlpha());
    }

    public void SetVirtualGaugeSpeciesSelection(bool showHerbivores, bool showPredators)
    {
        VirtualGaugeManager source = VirtualGaugePropertiesSource;
        bool visible = source == null || source.ShowVirtualGauges;
        SetVirtualGaugeVisibility(visible, showHerbivores, showPredators);
    }

    public void SetVirtualGaugeAlpha(float alpha)
    {
        VirtualGaugeManager source = VirtualGaugePropertiesSource;
        if (source == null)
            return;

        SetVirtualGaugeDisplaySettings(source.ShowHealthGauge, source.ShowEnergyGauge, source.ShowCarbonText, alpha);
    }

    public float GetVirtualGaugeAlpha()
    {
        VirtualGaugeManager source = VirtualGaugePropertiesSource;
        return source != null ? source.GaugeAlpha : 1f;
    }

    public void SetFocusCameraMode(bool keepCurrentRotation)
    {
        RotationThenlooking = keepCurrentRotation;
    }

    public bool IsFocusCameraKeepingCurrentRotation()
    {
        return RotationThenlooking;
    }

    public void MoveCameraToCenterTop(float heightOffset = 20f)
    {
        if (mainCamera == null)
            return;

        WorldGenerator source = TerrainPropertiesSource;
        if (source == null)
            return;

        mainCamera.transform.position = new Vector3(
            source.terrainSize * 0.5f,
            Mathf.Max(source.waterHeight + 1f, source.heightScale + Mathf.Max(0f, heightOffset)),
            source.terrainSize * 0.5f);
        mainCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        if (freeFlyCamera != null)
            freeFlyCamera.SyncRotationFromTransform();
    }

    public bool FocusRandomCreature()
    {
        int candidateCount = 0;
        GameObject[] candidates = new GameObject[2];

        if (herbivoreManager != null && herbivoreManager.TryGetComponent<herbivoreManager>(out var hm) && hm.herbivores != null)
        {
            GameObject herbivore = GetRandomAliveObject(hm.herbivores);
            if (herbivore != null)
                candidates[candidateCount++] = herbivore;
        }

        if (predatorManager != null && predatorManager.TryGetComponent<predatorManager>(out var pm) && pm.predators != null)
        {
            GameObject predator = GetRandomAliveObject(pm.predators);
            if (predator != null)
                candidates[candidateCount++] = predator;
        }

        if (candidateCount == 0)
            return false;

        SetTarget(candidates[Random.Range(0, candidateCount)]);
        return true;
    }

    public void SetGenerationSelectionSettings(
        EvaluationAxis evaluationAxisValue,
        GenomeInputMode herbivoreInputModeValue,
        GenomeInputMode predatorInputModeValue,
        GenerationPhase generationPhaseValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.evaluationAxis = evaluationAxisValue;
        source.herbivoreInputMode = herbivoreInputModeValue;
        source.predatorInputMode = predatorInputModeValue;
        source.generationPhase = generationPhaseValue;
    }

    public void SetGenerationEvaluationAxis(EvaluationAxis evaluationAxisValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.evaluationAxis = evaluationAxisValue;
    }

    public void SetGenerationInputModes(GenomeInputMode herbivoreInputModeValue, GenomeInputMode predatorInputModeValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.herbivoreInputMode = herbivoreInputModeValue;
        source.predatorInputMode = predatorInputModeValue;
    }

    public void SetGenerationTargetPhase(GenerationPhase generationPhaseValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.generationPhase = generationPhaseValue;
    }

    public void SetGenerationCrossoverSettings(bool enabled, CrossoverMode crossoverModeValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.enableCrossover = enabled;
        source.crossoverMode = crossoverModeValue;
    }

    public void SetGenerationCrossoverEnabled(bool enabled)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.enableCrossover = enabled;
    }

    public void SetGenerationCrossoverMode(CrossoverMode crossoverModeValue)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.crossoverMode = crossoverModeValue;
    }

    public void SetGenerationMutationSettings(
        bool enabled,
        float chance,
        MutationRangeMode rangeMode,
        float globalMin,
        float globalMax,
        float parentScale)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.enableMutation = enabled;
        source.mutationChance = Mathf.Clamp01(chance);
        source.mutationRangeMode = rangeMode;
        source.globalMutationMin = globalMin;
        source.globalMutationMax = globalMax;
        source.parentMutationScale = Mathf.Max(0f, parentScale);
    }

    public void SetGenerationMutationEnabled(bool enabled)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.enableMutation = enabled;
    }

    public void SetGenerationMutationChance(float chance)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.mutationChance = Mathf.Clamp01(chance);
    }

    public void SetGenerationMutationRange(float globalMin, float globalMax, float parentScale)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.globalMutationMin = globalMin;
        source.globalMutationMax = globalMax;
        source.parentMutationScale = Mathf.Max(0f, parentScale);
    }

    public void SetGenerationSaveOutput(bool saveOutput)
    {
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        source.saveOutputGenome = saveOutput;
    }

    public void SetGenerationSaveOutputEnabled(bool enabled)
    {
        SetGenerationSaveOutput(enabled);
    }

    public void SetPerformanceMonitorVisibility(bool overlayVisible)
    {
        PerformanceBudgetMonitor source = PerformanceMonitorSource;
        if (source == null)
            return;

        SetPerformanceMonitorVisibilityInternal(source, overlayVisible);
    }

    public void SetPerformanceDropLogging(bool enabled, float targetFps)
    {
        PerformanceBudgetMonitor source = PerformanceMonitorSource;
        if (source == null)
            return;

        SetPerformanceDropLoggingInternal(source, enabled, targetFps);
    }
}
