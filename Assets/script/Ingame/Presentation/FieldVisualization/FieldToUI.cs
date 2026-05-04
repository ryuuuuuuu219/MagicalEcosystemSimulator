using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldToUI : MonoBehaviour
{
    enum FieldView
    {
        None,
        ThreatMap,
        HeatField,
        ManaField,
        WindField
    }

    public WorldGenerator worldGenerator;
    public HeatFieldManager heatFieldManager;
    public ManaFieldManager manaFieldManager;
    public WindFieldManager windFieldManager;
    public threatmap_calc threatMap;
    public Camera uiCamera;
    public Transform screenUiParent;
    public Transform worldGridParent;
    public bool buildOwnCanvas = true;

    [Header("Grid")]
    public int gridResolution = 32;
    public float gridHeight = 30f;
    [Range(0f, 1f)] public float gridAlpha = 0.4f;
    [Range(0f, 0.2f)] public float gridCellGapRatio = 0.04f;
    public float updateInterval = 0.2f;

    [Header("Wind Lines")]
    public bool drawWindLines = true;
    public float windLineScale = 0.08f;
    public float windMinMagnitude = 0.02f;
    public float windLineWidth = 0.05f;

    FieldView currentView = FieldView.None;
    TMP_Dropdown dropdown;
    GameObject screenUiRoot;
    TextMeshProUGUI minText;
    TextMeshProUGUI maxText;
    Mesh gridMesh;
    GameObject gridObject;
    Color32[] gridColors;
    Vector3[] gridSamplePositions;
    float[] gridValues;
    float nextUpdateTime;
    RuntimeUpdater runtimeUpdater;
    readonly List<LineRenderer> windLines = new();
    Material windLineMaterial;

    static readonly Color LowColor = new Color(0.1f, 0.35f, 1f, 0.4f);
    static readonly Color MidColor = new Color(0.1f, 0.85f, 0.25f, 0.4f);
    static readonly Color HighColor = new Color(1f, 0.1f, 0.05f, 0.4f);

    void OnEnable()
    {
        if (gridObject != null)
            gridObject.SetActive(currentView != FieldView.None);
    }

    void OnDisable()
    {
        HideWindFieldLines();
    }

    void Start()
    {
        ResolveReferences();
        BuildScreenUI();
        BuildWorldGrid();
        EnsureRuntimeUpdater();
        UpdateFieldView(true);
    }

    void Update()
    {
        if (runtimeUpdater != null)
            return;

        RuntimeUpdate();
    }

    void RuntimeUpdate()
    {
        ResolveReferences();

        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + Mathf.Max(0.02f, updateInterval);
        UpdateFieldView(false);
    }

    void ResolveReferences()
    {
        if (worldGenerator == null)
            worldGenerator = FindFirstObjectByType<WorldGenerator>();
        if (heatFieldManager == null)
            heatFieldManager = HeatFieldManager.GetOrCreate();
        if (manaFieldManager == null)
            manaFieldManager = ManaFieldManager.GetOrCreate();
        if (windFieldManager == null)
            windFieldManager = WindFieldManager.GetOrCreate();
        if (threatMap == null)
            threatMap = FindFirstObjectByType<threatmap_calc>();
        if (uiCamera == null)
            uiCamera = Camera.main;
    }

    void BuildScreenUI()
    {
        EnsureEventSystem();

        Transform rootParent = screenUiParent != null ? screenUiParent : transform;
        if (buildOwnCanvas && screenUiParent == null)
        {
            screenUiRoot = new GameObject("FieldToUICanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = screenUiRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;

            CanvasScaler scaler = screenUiRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            screenUiRoot = new GameObject("FieldViewingPanel", typeof(RectTransform), typeof(Image));
            screenUiRoot.transform.SetParent(rootParent, false);
            RectTransform panelRect = screenUiRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(320f, 220f);
            screenUiRoot.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);
        }

        dropdown = CreateDropdown(screenUiRoot.transform, screenUiParent != null);
        CreateGauge(screenUiRoot.transform, screenUiParent != null);
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystem = new GameObject("FieldToUIEventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystem.transform.SetParent(transform, false);
    }

    TMP_Dropdown CreateDropdown(Transform parent, bool integratedLayout)
    {
        GameObject root = new GameObject("FieldDropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
        root.transform.SetParent(parent, false);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rect.anchorMax = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rect.pivot = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
        rect.anchoredPosition = integratedLayout ? new Vector2(16f, -16f) : new Vector2(-24f, -24f);
        rect.sizeDelta = new Vector2(260f, 42f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.08f, 0.9f);

        TextMeshProUGUI label = CreateText("Label", root.transform, "heat field", 18f, TextAlignmentOptions.MidlineLeft);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(14f, 0f);
        label.rectTransform.offsetMax = new Vector2(-36f, 0f);

        TextMeshProUGUI arrow = CreateText("Arrow", root.transform, "v", 18f, TextAlignmentOptions.Center);
        arrow.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        arrow.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        arrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.anchoredPosition = new Vector2(-18f, 0f);
        arrow.rectTransform.sizeDelta = new Vector2(24f, 28f);

        GameObject template = CreateDropdownTemplate(root.transform);
        TMP_Dropdown result = root.GetComponent<TMP_Dropdown>();
        result.targetGraphic = background;
        result.captionText = label;
        result.itemText = template.GetComponentInChildren<Toggle>(true).GetComponentInChildren<TextMeshProUGUI>(true);
        result.template = template.GetComponent<RectTransform>();
        result.options = new List<TMP_Dropdown.OptionData>
        {
            new("none"),
            new("threat map"),
            new("heat field"),
            new("mana field"),
            new("wind field")
        };
        result.value = 0;
        result.onValueChanged.AddListener(OnDropdownChanged);
        template.SetActive(false);
        return result;
    }

    GameObject CreateDropdownTemplate(Transform parent)
    {
        GameObject template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        RectTransform rect = template.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -4f);
        rect.sizeDelta = new Vector2(0f, 168f);
        template.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.05f, 0.95f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 166f);

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle), typeof(Image), typeof(LayoutElement));
        item.transform.SetParent(content.transform, false);
        item.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
        item.GetComponent<LayoutElement>().preferredHeight = 40f;

        TextMeshProUGUI itemText = CreateText("Item Label", item.transform, "option", 17f, TextAlignmentOptions.MidlineLeft);
        itemText.rectTransform.anchorMin = Vector2.zero;
        itemText.rectTransform.anchorMax = Vector2.one;
        itemText.rectTransform.offsetMin = new Vector2(14f, 0f);
        itemText.rectTransform.offsetMax = new Vector2(-10f, 0f);

        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = item.GetComponent<Image>();

        ScrollRect scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = false;

        return template;
    }

    void CreateGauge(Transform parent, bool integratedLayout)
    {
        GameObject gauge = new GameObject("FieldColorGauge", typeof(RectTransform), typeof(RawImage));
        gauge.transform.SetParent(parent, false);
        RectTransform rect = gauge.GetComponent<RectTransform>();
        rect.anchorMin = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 0.5f);
        rect.anchorMax = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = integratedLayout ? new Vector2(286f, -126f) : new Vector2(-150f, 0f);
        rect.sizeDelta = new Vector2(22f, integratedLayout ? 148f : 260f);

        Texture2D texture = new Texture2D(1, 128, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < texture.height; y++)
        {
            float t = y / (texture.height - 1f);
            texture.SetPixel(0, y, EvaluateGradient(t, 1f));
        }
        texture.Apply(false, true);

        RawImage image = gauge.GetComponent<RawImage>();
        image.texture = texture;
        image.color = Color.white;

        maxText = CreateText("FieldMaxText", parent, "max 0.00", 15f, TextAlignmentOptions.MidlineLeft);
        ConfigureGaugeLabel(maxText.rectTransform, integratedLayout, new Vector2(16f, -78f), new Vector2(-142f, 130f));

        minText = CreateText("FieldMinText", parent, "min 0.00", 15f, TextAlignmentOptions.MidlineLeft);
        ConfigureGaugeLabel(minText.rectTransform, integratedLayout, new Vector2(16f, -176f), new Vector2(-142f, -130f));
    }

    void ConfigureGaugeLabel(RectTransform rect, bool integratedLayout, Vector2 integratedPosition, Vector2 overlayPosition)
    {
        rect.anchorMin = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 0.5f);
        rect.anchorMax = integratedLayout ? new Vector2(0f, 1f) : new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = integratedLayout ? integratedPosition : overlayPosition;
        rect.sizeDelta = new Vector2(120f, 24f);
    }

    TextMeshProUGUI CreateText(string name, Transform parent, string value, float size, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    void BuildWorldGrid()
    {
        int safeResolution = Mathf.Clamp(gridResolution, 4, 96);
        int cellCount = safeResolution * safeResolution;
        gridValues = new float[cellCount];
        gridSamplePositions = new Vector3[cellCount];
        gridColors = new Color32[cellCount * 4];

        Vector3[] vertices = new Vector3[cellCount * 4];
        int[] triangles = new int[cellCount * 6];
        float terrainSize = worldGenerator != null ? worldGenerator.terrainSize : 220f;
        float cellSize = terrainSize / safeResolution;
        float gap = Mathf.Clamp01(gridCellGapRatio) * cellSize;
        int vi = 0;
        int ti = 0;
        int ci = 0;

        for (int z = 0; z < safeResolution; z++)
        {
            for (int x = 0; x < safeResolution; x++)
            {
                float x0 = x * cellSize + gap;
                float z0 = z * cellSize + gap;
                float x1 = (x + 1) * cellSize - gap;
                float z1 = (z + 1) * cellSize - gap;

                vertices[vi + 0] = new Vector3(x0, gridHeight, z0);
                vertices[vi + 1] = new Vector3(x1, gridHeight, z0);
                vertices[vi + 2] = new Vector3(x1, gridHeight, z1);
                vertices[vi + 3] = new Vector3(x0, gridHeight, z1);

                triangles[ti + 0] = vi + 0;
                triangles[ti + 1] = vi + 2;
                triangles[ti + 2] = vi + 1;
                triangles[ti + 3] = vi + 0;
                triangles[ti + 4] = vi + 3;
                triangles[ti + 5] = vi + 2;

                gridSamplePositions[ci] = new Vector3((x0 + x1) * 0.5f, gridHeight, (z0 + z1) * 0.5f);
                vi += 4;
                ti += 6;
                ci++;
            }
        }

        gridMesh = new Mesh
        {
            name = "FieldToUIGridMesh",
            vertices = vertices,
            triangles = triangles,
            colors32 = gridColors
        };
        gridMesh.RecalculateBounds();

        GameObject gridObj = new GameObject("FieldToUI Virtual Grid", typeof(MeshFilter), typeof(MeshRenderer));
        gridObject = gridObj;
        gridObj.transform.SetParent(worldGridParent, false);
        gridObj.transform.position = Vector3.zero;
        gridObj.transform.rotation = Quaternion.identity;
        gridObj.transform.localScale = Vector3.one;
        gridObj.GetComponent<MeshFilter>().sharedMesh = gridMesh;

        Shader shader = Shader.Find("MagicalEcosystem/Experiment/FieldGridVertexColor");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        Material material = new Material(shader);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        gridObj.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    void OnDropdownChanged(int value)
    {
        currentView = value switch
        {
            1 => FieldView.ThreatMap,
            2 => FieldView.HeatField,
            3 => FieldView.ManaField,
            4 => FieldView.WindField,
            _ => FieldView.None
        };
        UpdateFieldView(true);
    }

    void UpdateFieldView(bool force)
    {
        if (gridMesh == null || gridSamplePositions == null)
            return;

        if (currentView == FieldView.None)
        {
            if (gridObject != null)
                gridObject.SetActive(false);
            HideWindFieldLines();
            if (minText != null)
                minText.text = "min -";
            if (maxText != null)
                maxText.text = "max -";
            return;
        }

        if (gridObject != null && !gridObject.activeSelf)
            gridObject.SetActive(true);

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        for (int i = 0; i < gridSamplePositions.Length; i++)
        {
            float value = SampleCurrentField(gridSamplePositions[i]);
            gridValues[i] = value;
            min = Mathf.Min(min, value);
            max = Mathf.Max(max, value);
        }

        if (!float.IsFinite(min) || !float.IsFinite(max))
        {
            min = 0f;
            max = 0f;
        }

        bool crossesZero = min * max < 0f;
        float range = Mathf.Max(0.0001f, max - min);
        for (int i = 0; i < gridValues.Length; i++)
        {
            float t = crossesZero
                ? EvaluateSignedGradientPosition(gridValues[i], min, max)
                : Mathf.Clamp01((gridValues[i] - min) / range);
            Color32 color = EvaluateGradient(t, gridAlpha);
            int vi = i * 4;
            gridColors[vi + 0] = color;
            gridColors[vi + 1] = color;
            gridColors[vi + 2] = color;
            gridColors[vi + 3] = color;
        }

        gridMesh.colors32 = gridColors;
        if (minText != null)
            minText.text = $"min {min:0.00}";
        if (maxText != null)
            maxText.text = $"max {max:0.00}";

        DrawWindFieldLines();
    }

    static float EvaluateSignedGradientPosition(float value, float min, float max)
    {
        if (value < 0f)
        {
            float negativeRange = Mathf.Max(0.0001f, Mathf.Abs(min));
            return Mathf.Lerp(0f, 0.5f, Mathf.Clamp01((value - min) / negativeRange));
        }

        float positiveRange = Mathf.Max(0.0001f, max);
        return Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(value / positiveRange));
    }

    float SampleCurrentField(Vector3 position)
    {
        return currentView switch
        {
            FieldView.ThreatMap => threatMap != null ? threatMap.SampleEvaluatedThreat(position) : 0f,
            FieldView.ManaField => manaFieldManager != null ? manaFieldManager.SampleMana(position) : 0f,
            FieldView.WindField => windFieldManager != null ? windFieldManager.SampleWind(position).magnitude : 0f,
            _ => heatFieldManager != null ? heatFieldManager.SampleHeat(position) : 0f
        };
    }

    void DrawWindFieldLines()
    {
        if (!drawWindLines || currentView != FieldView.WindField || windFieldManager == null || gridSamplePositions == null)
        {
            HideWindFieldLines();
            return;
        }

        float scale = Mathf.Max(0.001f, windLineScale);
        int visibleCount = 0;
        for (int i = 0; i < gridSamplePositions.Length; i++)
        {
            Vector3 start = gridSamplePositions[i];
            Vector2 wind = windFieldManager.SampleWind(start);
            if (wind.magnitude < windMinMagnitude)
                continue;

            Vector3 end = start + new Vector3(wind.x, 0f, wind.y) * scale;
            LineRenderer line = EnsureWindLine(visibleCount++);
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = windLineWidth;
            line.endWidth = Mathf.Max(0.01f, windLineWidth * 0.35f);
            line.enabled = true;
        }

        for (int i = visibleCount; i < windLines.Count; i++)
        {
            if (windLines[i] != null)
                windLines[i].enabled = false;
        }
    }

    LineRenderer EnsureWindLine(int index)
    {
        while (windLines.Count <= index)
        {
            GameObject lineObj = new GameObject("FieldToUI Wind Vector Line", typeof(LineRenderer));
            Transform parent = gridObject != null ? gridObject.transform : transform;
            lineObj.transform.SetParent(parent, false);

            LineRenderer line = lineObj.GetComponent<LineRenderer>();
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.numCapVertices = 2;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = GetWindLineMaterial();
            line.startColor = new Color(0.35f, 1f, 0.55f, 0.85f);
            line.endColor = new Color(0.85f, 1f, 0.35f, 0.15f);
            windLines.Add(line);
        }

        return windLines[index];
    }

    Material GetWindLineMaterial()
    {
        if (windLineMaterial != null)
            return windLineMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        windLineMaterial = new Material(shader);
        if (windLineMaterial.HasProperty("_BaseColor"))
            windLineMaterial.SetColor("_BaseColor", Color.green);
        else if (windLineMaterial.HasProperty("_Color"))
            windLineMaterial.SetColor("_Color", Color.green);
        windLineMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return windLineMaterial;
    }

    void HideWindFieldLines()
    {
        for (int i = 0; i < windLines.Count; i++)
        {
            if (windLines[i] != null)
                windLines[i].enabled = false;
        }
    }

    void EnsureRuntimeUpdater()
    {
        if (runtimeUpdater != null)
            return;

        GameObject updaterObject = new GameObject("FieldToUI Runtime Updater");
        updaterObject.transform.SetParent(worldGridParent, false);
        runtimeUpdater = updaterObject.AddComponent<RuntimeUpdater>();
        runtimeUpdater.Initialize(this);
    }

    static Color EvaluateGradient(float t, float alpha)
    {
        Color color = t < 0.5f
            ? Color.Lerp(LowColor, MidColor, t * 2f)
            : Color.Lerp(MidColor, HighColor, (t - 0.5f) * 2f);
        color.a = alpha;
        return color;
    }

    sealed class RuntimeUpdater : MonoBehaviour
    {
        FieldToUI owner;

        public void Initialize(FieldToUI target)
        {
            owner = target;
        }

        void Update()
        {
            if (owner != null)
                owner.RuntimeUpdate();
        }

        void OnDestroy()
        {
            if (owner != null && owner.runtimeUpdater == this)
                owner.runtimeUpdater = null;
        }
    }
}
