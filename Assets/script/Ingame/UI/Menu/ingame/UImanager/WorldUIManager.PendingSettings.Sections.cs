using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class WorldUIManager
{
    void RebuildPendingSettingsContent()
    {
        EnsurePendingSettingsPanel();
        if (pendingSettingsContentRoot == null)
            return;

        for (int i = pendingSettingsContentRoot.childCount - 1; i >= 0; i--)
            Destroy(pendingSettingsContentRoot.GetChild(i).gameObject);

        if (pendingSettingsTitleText != null)
            pendingSettingsTitleText.text = pendingSettingsView == PendingSettingsView.GenerationOnly
                ? "Generation"
                : "Pending Settings";

        if (pendingSettingsView == PendingSettingsView.GenerationOnly)
        {
            BuildGenerationSection(pendingSettingsContentRoot);
            return;
        }

        BuildEnvironmentSection(pendingSettingsContentRoot);
        BuildUiSection(pendingSettingsContentRoot);
        BuildCameraSection(pendingSettingsContentRoot);
        BuildGenerationSection(pendingSettingsContentRoot);
        BuildDeferredSection(pendingSettingsContentRoot);
    }

    void BuildEnvironmentSection(Transform parent)
    {
        Transform section = CreateSettingsSection(parent, "Environment Adjustment");
        AddStepperRow(section, "Time Scale", GetTimeScale().ToString("F2"),
            () => { SetTimeScale(Mathf.Max(0f, GetTimeScale() - 0.25f)); RebuildPendingSettingsContent(); },
            () => { SetTimeScale(GetTimeScale() + 0.25f); RebuildPendingSettingsContent(); });

        WorldGenerator terrainSource = TerrainPropertiesSource;
        if (terrainSource != null)
        {
            AddStepperRow(section, "Terrain Size", terrainSource.terrainSize.ToString(),
                () => { SetTerrainParameters(terrainSource.terrainSize - 32, terrainSource.heightmapResolution, terrainSource.heightScale, terrainSource.noiseScale, terrainSource.octaves, terrainSource.persistence, terrainSource.lacunarity, terrainSource.waterHeight); RebuildPendingSettingsContent(); },
                () => { SetTerrainParameters(terrainSource.terrainSize + 32, terrainSource.heightmapResolution, terrainSource.heightScale, terrainSource.noiseScale, terrainSource.octaves, terrainSource.persistence, terrainSource.lacunarity, terrainSource.waterHeight); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Water Height", terrainSource.waterHeight.ToString("F1"),
                () => { SetTerrainParameters(terrainSource.terrainSize, terrainSource.heightmapResolution, terrainSource.heightScale, terrainSource.noiseScale, terrainSource.octaves, terrainSource.persistence, terrainSource.lacunarity, terrainSource.waterHeight - 1f); RebuildPendingSettingsContent(); },
                () => { SetTerrainParameters(terrainSource.terrainSize, terrainSource.heightmapResolution, terrainSource.heightScale, terrainSource.noiseScale, terrainSource.octaves, terrainSource.persistence, terrainSource.lacunarity, terrainSource.waterHeight + 1f); RebuildPendingSettingsContent(); });
        }

        ResourceDispenser spawnSource = SpawnPropertiesSource;
        if (spawnSource != null)
        {
            AddStepperRow(section, "Initial Herbivore", spawnSource.initialHerbivoreCount.ToString(),
                () => { SetInitialSpawnCounts(spawnSource.initialGrassCount, spawnSource.initialHerbivoreCount - 5, spawnSource.initialPredatorCount); RebuildPendingSettingsContent(); },
                () => { SetInitialSpawnCounts(spawnSource.initialGrassCount, spawnSource.initialHerbivoreCount + 5, spawnSource.initialPredatorCount); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Initial Predator", spawnSource.initialPredatorCount.ToString(),
                () => { SetInitialSpawnCounts(spawnSource.initialGrassCount, spawnSource.initialHerbivoreCount, spawnSource.initialPredatorCount - 2); RebuildPendingSettingsContent(); },
                () => { SetInitialSpawnCounts(spawnSource.initialGrassCount, spawnSource.initialHerbivoreCount, spawnSource.initialPredatorCount + 2); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Gen Grass Count", spawnSource.grassCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration - 10, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration + 10, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Plant Radius", spawnSource.radius.ToString("F1"),
                () => { SetVegetationSpawnParameters(spawnSource.radius - 1f, spawnSource.density, spawnSource.plantCount, spawnSource.maxSlope); RebuildPendingSettingsContent(); },
                () => { SetVegetationSpawnParameters(spawnSource.radius + 1f, spawnSource.density, spawnSource.plantCount, spawnSource.maxSlope); RebuildPendingSettingsContent(); });
        }
    }

    void BuildUiSection(Transform parent)
    {
        Transform section = CreateSettingsSection(parent, "UI");
        AddActionRow(section, $"Selectable Layer: {SelectableLayerSource.value}",
            ("Reset", () => { SetSelectableLayer(defaultSelectableLayer); RebuildPendingSettingsContent(); }),
            ("All", () => { SetSelectableLayer(~0); RebuildPendingSettingsContent(); }));

        VirtualGaugeManager gaugeSource = VirtualGaugePropertiesSource;
        if (gaugeSource != null)
        {
            AddActionRow(section, $"Gauge Status H:{ToOnOff(gaugeSource.ShowHealthGauge)} Mana:{ToOnOff(gaugeSource.ShowManaGauge)} Text:{ToOnOff(gaugeSource.ShowManaText)}",
                ("Toggle HP", () => { SetVirtualGaugeStatusSelection(!gaugeSource.ShowHealthGauge, gaugeSource.ShowManaText, gaugeSource.ShowManaGauge); RebuildPendingSettingsContent(); }),
                ("Toggle Text", () => { SetVirtualGaugeStatusSelection(gaugeSource.ShowHealthGauge, !gaugeSource.ShowManaText, gaugeSource.ShowManaGauge); RebuildPendingSettingsContent(); }),
                ("Toggle Mana", () => { SetVirtualGaugeStatusSelection(gaugeSource.ShowHealthGauge, gaugeSource.ShowManaText, !gaugeSource.ShowManaGauge); RebuildPendingSettingsContent(); }));

            AddActionRow(section, $"Gauge Species H:{ToOnOff(gaugeSource.ShowHerbivoreGauges)} P:{ToOnOff(gaugeSource.ShowPredatorGauges)}",
                ("Herb", () => { SetVirtualGaugeSpeciesSelection(!gaugeSource.ShowHerbivoreGauges, gaugeSource.ShowPredatorGauges); RebuildPendingSettingsContent(); }),
                ("Pred", () => { SetVirtualGaugeSpeciesSelection(gaugeSource.ShowHerbivoreGauges, !gaugeSource.ShowPredatorGauges); RebuildPendingSettingsContent(); }));

            AddStepperRow(section, "Gauge Alpha", gaugeSource.GaugeAlpha.ToString("F2"),
                () => { SetVirtualGaugeAlpha(gaugeSource.GaugeAlpha - 0.1f); RebuildPendingSettingsContent(); },
                () => { SetVirtualGaugeAlpha(gaugeSource.GaugeAlpha + 0.1f); RebuildPendingSettingsContent(); });
        }
    }

    void BuildCameraSection(Transform parent)
    {
        Transform section = CreateSettingsSection(parent, "Camera Operation");
        AddActionRow(section, $"Focus Mode: {(IsFocusCameraKeepingCurrentRotation() ? "Keep Rotation" : "Top Down")}",
            ("Toggle", () => { SetFocusCameraMode(!IsFocusCameraKeepingCurrentRotation()); RebuildPendingSettingsContent(); }));

        AddActionRow(section, "Presets",
            ("Center Top", () => { MoveCameraToCenterTop(); RebuildPendingSettingsContent(); }),
            ("Random Focus", () => { FocusRandomCreature(); RebuildPendingSettingsContent(); }));
    }

    void BuildGenerationSection(Transform parent)
    {
        Transform section = CreateSettingsSection(parent, "Generation");
        AdvanceGenerationController source = GenerationPropertiesSource;
        if (source == null)
            return;

        AddActionRow(section, $"Evaluation Axis: {source.evaluationAxis}",
            ("Cycle", () => { SetGenerationEvaluationAxis(GetNextEnumValue(source.evaluationAxis)); RebuildPendingSettingsContent(); }));

        AddActionRow(section, $"Input Modes: H={source.herbivoreInputMode} P={source.predatorInputMode}",
            ("Herb", () => { SetGenerationInputModes(GetNextEnumValue(source.herbivoreInputMode), source.predatorInputMode); RebuildPendingSettingsContent(); }),
            ("Pred", () => { SetGenerationInputModes(source.herbivoreInputMode, GetNextEnumValue(source.predatorInputMode)); RebuildPendingSettingsContent(); }));

        AddActionRow(section, $"Target Phase: {source.generationPhase}",
            ("Cycle", () => { SetGenerationTargetPhase(GetNextEnumValue(source.generationPhase)); RebuildPendingSettingsContent(); }));

        ResourceDispenser spawnSource = SpawnPropertiesSource;
        if (spawnSource != null)
        {
            AddInfoRow(section, BuildPhasePopulationSummary());

            AddStepperRow(section, "Gen Grass", spawnSource.grassCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration - 10, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration + 10, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Gen Herbivore", spawnSource.herbivoreCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration - 5, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration + 5, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Gen Predator", spawnSource.predatorCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration - 2, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration + 2, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Gen High Predator", spawnSource.highPredatorCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration - 1, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration + 1, spawnSource.dominantCountPerGeneration); RebuildPendingSettingsContent(); });

            AddStepperRow(section, "Gen Dominant", spawnSource.dominantCountPerGeneration.ToString(),
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration - 1); RebuildPendingSettingsContent(); },
                () => { SetGenerationSpawnCounts(spawnSource.grassCountPerGeneration, spawnSource.herbivoreCountPerGeneration, spawnSource.predatorCountPerGeneration, spawnSource.highPredatorCountPerGeneration, spawnSource.dominantCountPerGeneration + 1); RebuildPendingSettingsContent(); });
        }

        AddActionRow(section, $"Crossover: {ToOnOff(source.enableCrossover)} / {source.crossoverMode}",
            ("Toggle", () => { SetGenerationCrossoverEnabled(!source.enableCrossover); RebuildPendingSettingsContent(); }),
            ("Mode", () => { SetGenerationCrossoverMode(GetNextEnumValue(source.crossoverMode)); RebuildPendingSettingsContent(); }));

        AddActionRow(section, $"Mutation: {ToOnOff(source.enableMutation)} / {source.mutationRangeMode}",
            ("Toggle", () => { SetGenerationMutationEnabled(!source.enableMutation); RebuildPendingSettingsContent(); }),
            ("Range", () => { SetGenerationMutationSettings(source.enableMutation, source.mutationChance, GetNextEnumValue(source.mutationRangeMode), source.globalMutationMin, source.globalMutationMax, source.parentMutationScale); RebuildPendingSettingsContent(); }));

        AddStepperRow(section, "Mutation Chance", source.mutationChance.ToString("F2"),
            () => { SetGenerationMutationChance(source.mutationChance - 0.05f); RebuildPendingSettingsContent(); },
            () => { SetGenerationMutationChance(source.mutationChance + 0.05f); RebuildPendingSettingsContent(); });

        AddStepperRow(section, "Parent Range Scale", source.parentMutationScale.ToString("F2"),
            () => { SetGenerationMutationRange(source.globalMutationMin, source.globalMutationMax, source.parentMutationScale - 0.05f); RebuildPendingSettingsContent(); },
            () => { SetGenerationMutationRange(source.globalMutationMin, source.globalMutationMax, source.parentMutationScale + 0.05f); RebuildPendingSettingsContent(); });

        AddActionRow(section, $"Save Output: {ToOnOff(source.saveOutputGenome)}",
            ("Toggle", () => { SetGenerationSaveOutputEnabled(!source.saveOutputGenome); RebuildPendingSettingsContent(); }));

        AddFullWidthActionRow(
            section,
            "Run Generation",
            () => { source.onclickbutton2_1(); RebuildPendingSettingsContent(); },
            new Color(1f, 0.42f, 0.62f, 0.95f));
    }

    void BuildDeferredSection(Transform parent)
    {
        Transform section = CreateSettingsSection(parent, "Deferred");
        PerformanceBudgetMonitor source = PerformanceMonitorSource;
        bool overlayVisible = source != null && GetPerformanceOverlayVisible(source);
        bool dropLogEnabled = source != null && GetPerformanceDropLoggingEnabled(source);
        float watchTarget = source != null ? GetPerformanceWatchTarget(source) : 60f;

        AddActionRow(section, $"Perf Overlay: {ToOnOff(overlayVisible)}",
            ("Toggle", () => { SetPerformanceMonitorVisibility(!overlayVisible); RebuildPendingSettingsContent(); }));

        AddActionRow(section, $"Drop Log: {ToOnOff(dropLogEnabled)} @ {watchTarget:F0}fps",
            ("Toggle", () => { SetPerformanceDropLogging(!dropLogEnabled, watchTarget); RebuildPendingSettingsContent(); }),
            ("Target+", () => { SetPerformanceDropLogging(dropLogEnabled, watchTarget + 5f); RebuildPendingSettingsContent(); }));
    }

    Transform CreateSettingsSection(Transform parent, string title)
    {
        GameObject section = new GameObject(title.Replace(" ", string.Empty) + "_Section", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        section.transform.SetParent(parent, false);
        section.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
        RectTransform rect = section.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = section.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 4f;
        layout.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter fitter = section.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        TextMeshProUGUI header = CreateSettingsLabel(section.transform, "Header", title, 18f, TextAlignmentOptions.MidlineLeft);
        header.color = new Color(1f, 0.9f, 0.6f, 1f);
        return section.transform;
    }

    void AddStepperRow(Transform parent, string label, string value, Action onDecrease, Action onIncrease)
    {
        AddActionRow(parent, $"{label}: {value}",
            ("-", () => onDecrease?.Invoke()),
            ("+", () => onIncrease?.Invoke()));
    }

    void AddInfoRow(Transform parent, string label)
    {
        GameObject row = new GameObject("InfoRow", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);
        LayoutElement layout = row.GetComponent<LayoutElement>();
        layout.preferredHeight = 34f;

        TextMeshProUGUI rowLabel = CreateSettingsLabel(row.transform, "Label", label, 15f, TextAlignmentOptions.MidlineLeft);
        rowLabel.textWrappingMode = TextWrappingModes.Normal;
    }

    void AddFullWidthActionRow(Transform parent, string text, Action action, Color buttonColor)
    {
        GameObject row = new GameObject(text.Replace(" ", string.Empty) + "_Row", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);

        LayoutElement rowLayout = row.GetComponent<LayoutElement>();
        rowLayout.preferredHeight = 36f;

        Button button = CreateSettingsButton(row.transform, text, action);
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.offsetMin = new Vector2(4f, 4f);
        buttonRect.offsetMax = new Vector2(-4f, -4f);

        if (button.TryGetComponent<Image>(out var image))
            image.color = buttonColor;

        if (button.TryGetComponent<LayoutElement>(out var buttonLayout))
        {
            buttonLayout.ignoreLayout = true;
            buttonLayout.preferredWidth = 0f;
            buttonLayout.preferredHeight = 0f;
        }
    }

    void AddActionRow(Transform parent, string label, params (string text, Action action)[] actions)
    {
        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        row.transform.SetParent(parent, false);
        row.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.025f);
        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(1f, 1f);
        rowRect.pivot = new Vector2(0.5f, 1f);
        rowRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup rowLayout = row.GetComponent<VerticalLayoutGroup>();
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = true;
        rowLayout.childForceExpandHeight = false;
        rowLayout.spacing = 2f;
        rowLayout.padding = new RectOffset(4, 4, 4, 4);

        ContentSizeFitter rowFitter = row.GetComponent<ContentSizeFitter>();
        rowFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rowFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        TextMeshProUGUI rowLabel = CreateSettingsLabel(row.transform, "Label", label, 15f, TextAlignmentOptions.MidlineLeft);
        rowLabel.textWrappingMode = TextWrappingModes.Normal;

        GameObject buttonLine = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        buttonLine.transform.SetParent(row.transform, false);
        RectTransform buttonRect = buttonLine.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup buttonLayout = buttonLine.GetComponent<HorizontalLayoutGroup>();
        buttonLayout.childControlWidth = false;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = false;
        buttonLayout.childForceExpandHeight = false;
        buttonLayout.spacing = 4f;

        ContentSizeFitter buttonFitter = buttonLine.GetComponent<ContentSizeFitter>();
        buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < actions.Length; i++)
            CreateSettingsButton(buttonLine.transform, actions[i].text, actions[i].action);
    }

    Button CreateSettingsButton(Transform parent, string text, Action onClick)
    {
        GameObject buttonObj;
        if (buttonPrefab != null)
        {
            buttonObj = Instantiate(buttonPrefab, parent);
            buttonObj.name = text + "_Button";
        }
        else
        {
            buttonObj = new GameObject(text + "_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObj.transform.SetParent(parent, false);
            buttonObj.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.2f, 0.9f);
            buttonObj.GetComponent<LayoutElement>().preferredWidth = 80f;
            buttonObj.GetComponent<LayoutElement>().preferredHeight = 24f;
            CreateSettingsLabel(buttonObj.transform, "Label", text, 14f, TextAlignmentOptions.Center);
        }

        LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
        if (layout == null)
            layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredWidth = Mathf.Max(68f, text.Length * 10f);
        layout.preferredHeight = 24f;

        TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = text;

        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
            button = buttonObj.AddComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
        return button;
    }

    TextMeshProUGUI CreateSettingsLabel(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject labelObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        labelObj.transform.SetParent(parent, false);
        LayoutElement layout = labelObj.GetComponent<LayoutElement>();
        layout.preferredHeight = fontSize + 14f;
        layout.flexibleWidth = 1f;

        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = Vector2.zero;

        TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.overflowMode = TextOverflowModes.Overflow;
        return label;
    }

    static string ToOnOff(bool value)
    {
        return value ? "On" : "Off";
    }

    static T GetNextEnumValue<T>(T current) where T : struct
    {
        Array values = Enum.GetValues(typeof(T));
        int index = Array.IndexOf(values, current);
        if (index < 0)
            return (T)values.GetValue(0);

        int nextIndex = (index + 1) % values.Length;
        return (T)values.GetValue(nextIndex);
    }
}
