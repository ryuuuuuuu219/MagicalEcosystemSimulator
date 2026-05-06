using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class WorldUIManager
{
    [Header("Virtual Gauge")]
    [SerializeField] float virtualGaugePlaneDistance = 2.5f;
    [SerializeField] float selectedGaugeScale = 1.3f;

    VirtualGaugeManager virtualGaugeManager;
    Canvas virtualGaugeCanvas;
    RectTransform virtualGaugeCanvasRect;
    readonly Dictionary<CreatureVirtualGauge, GaugeView> gaugeViews = new();
    readonly List<CreatureVirtualGauge> gaugeTargets = new();

    sealed class GaugeView
    {
        public RectTransform Root;
        public Slider HealthSlider;
        public Slider ManaSlider;
        public Slider ResourceManaSlider;
        public TextMeshProUGUI ManaLapText;
        public Image HealthBackground;
        public Image ManaBackground;
        public Image ResourceManaBackground;
        public Image HealthFill;
        public Image ManaFill;
        public Image ResourceManaFill;
        public Color HealthBackgroundColor;
        public Color ManaBackgroundColor;
        public Color ResourceManaBackgroundColor;
        public Color HealthFillColor;
        public Color ManaFillColor;
        public Color ResourceManaFillColor;
        public Color ManaLapTextColor;
    }

    void InitializeVirtualGaugeCanvas()
    {
        if (mainCamera == null || virtualGaugeCanvas != null)
            return;

        GameObject canvasObj = new GameObject("VirtualGaugeCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObj.transform.SetParent(mainCamera.transform, false);

        virtualGaugeCanvas = canvasObj.GetComponent<Canvas>();
        virtualGaugeCanvas.renderMode = RenderMode.WorldSpace;
        virtualGaugeCanvas.worldCamera = mainCamera;
        virtualGaugeCanvas.sortingOrder = 200;

        virtualGaugeCanvasRect = canvasObj.GetComponent<RectTransform>();
        virtualGaugeCanvasRect.sizeDelta = new Vector2(1920f, 1080f);
        virtualGaugeCanvasRect.localPosition = new Vector3(0f, 0f, virtualGaugePlaneDistance);
        virtualGaugeCanvasRect.localRotation = Quaternion.identity;
        virtualGaugeCanvasRect.localScale = Vector3.one * 0.0025f;
    }

    void InitializeVirtualGaugeManager()
    {
        if (virtualGaugeManager != null)
            return;

        virtualGaugeManager = VirtualGaugeManager.Instance;
        if (virtualGaugeManager == null)
            virtualGaugeManager = GetComponent<VirtualGaugeManager>();
        if (virtualGaugeManager == null)
            virtualGaugeManager = gameObject.AddComponent<VirtualGaugeManager>();
    }

    void UpdateVirtualGaugeVisibility()
    {
        bool visible = IsVirtualGaugeVisible();
        if (virtualGaugeCanvas != null)
            virtualGaugeCanvas.enabled = visible;
    }

    bool IsVirtualGaugeVisible()
    {
        return virtualGaugeManager == null || virtualGaugeManager.ShowVirtualGauges;
    }

    void UpdateVirtualGauges()
    {
        InitializeVirtualGaugeManager();
        InitializeVirtualGaugeCanvas();
        UpdateVirtualGaugeVisibility();

        if (!IsVirtualGaugeVisible() || mainCamera == null || virtualGaugeCanvasRect == null)
        {
            HideAllGaugeViews();
            return;
        }

        CollectGaugeTargets();
        SyncGaugeViews();

        Plane canvasPlane = new Plane(mainCamera.transform.forward, virtualGaugeCanvasRect.position);

        foreach (var pair in gaugeViews)
        {
            CreatureVirtualGauge gauge = pair.Key;
            GaugeView view = pair.Value;

            if (gauge == null || !gauge.isActiveAndEnabled || !gauge.IsAliveTarget)
            {
                if (view.Root != null)
                    view.Root.gameObject.SetActive(false);
                continue;
            }

            if (!ShouldShowGaugeFor(gauge))
            {
                view.Root.gameObject.SetActive(false);
                continue;
            }

            if (!gauge.TryGetRatios(out float healthRatio, out float manaRatio, out float resourceManaRatio, out int manaLap))
            {
                view.Root.gameObject.SetActive(false);
                continue;
            }

            Vector3 anchor = gauge.GetAnchorWorldPosition();
            Vector3 rayDir = anchor - mainCamera.transform.position;
            float dirDot = Vector3.Dot(mainCamera.transform.forward, rayDir.normalized);
            if (dirDot <= 0.01f)
            {
                view.Root.gameObject.SetActive(false);
                continue;
            }

            Ray ray = new Ray(mainCamera.transform.position, rayDir.normalized);
            if (!canvasPlane.Raycast(ray, out float enter))
            {
                view.Root.gameObject.SetActive(false);
                continue;
            }

            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 localPoint = virtualGaugeCanvasRect.InverseTransformPoint(hitPoint);

            view.Root.gameObject.SetActive(true);
            view.Root.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
            float scale = currentTarget == gauge.gameObject ? selectedGaugeScale : 1f;
            view.Root.localScale = Vector3.one * scale;
            ApplyGaugeAlpha(view, virtualGaugeManager == null ? 1f : virtualGaugeManager.GaugeAlpha);
            ApplyManaOverflowColor(view, manaLap > 0);
            view.HealthSlider.value = healthRatio;
            view.ManaSlider.value = manaRatio;
            view.ResourceManaSlider.value = resourceManaRatio;
            view.HealthSlider.gameObject.SetActive(virtualGaugeManager == null || virtualGaugeManager.ShowHealthGauge);
            view.ManaSlider.gameObject.SetActive(virtualGaugeManager == null || virtualGaugeManager.ShowManaGauge);
            view.ResourceManaSlider.gameObject.SetActive(virtualGaugeManager == null || virtualGaugeManager.ShowManaText);
            if (view.ManaLapText != null)
            {
                bool showManaLap = (virtualGaugeManager == null || virtualGaugeManager.ShowManaGauge) && manaLap > 0;
                view.ManaLapText.gameObject.SetActive(showManaLap);
                view.ManaLapText.text = $"{manaLap + 1}x";
            }
        }
    }

    void ApplyManaOverflowColor(GaugeView view, bool overflow)
    {
        if (!overflow)
            return;

        float manaAlpha = view.ManaFill != null ? view.ManaFill.color.a : 1f;
        float resourceAlpha = view.ResourceManaFill != null ? view.ResourceManaFill.color.a : 1f;
        if (view.ManaFill != null)
            view.ManaFill.color = new Color(1f, 0.22f, 0.08f, manaAlpha);
        if (view.ResourceManaFill != null)
            view.ResourceManaFill.color = new Color(1f, 0.48f, 0.1f, resourceAlpha);
    }

    bool ShouldShowGaugeFor(CreatureVirtualGauge gauge)
    {
        if (virtualGaugeManager == null)
            return true;

        if (gauge.IsHerbivore && !virtualGaugeManager.ShowHerbivoreGauges)
            return false;
        if (gauge.IsPredator && !virtualGaugeManager.ShowPredatorGauges)
            return false;

        return virtualGaugeManager.ShowHealthGauge || virtualGaugeManager.ShowManaGauge || virtualGaugeManager.ShowManaText;
    }

    void HideAllGaugeViews()
    {
        foreach (var pair in gaugeViews)
        {
            if (pair.Value.Root != null)
                pair.Value.Root.gameObject.SetActive(false);
        }
    }

    void CollectGaugeTargets()
    {
        gaugeTargets.Clear();

        if (herbivoreManager != null && herbivoreManager.TryGetComponent<herbivoreManager>(out var hm))
        {
            for (int i = 0; i < hm.herbivores.Count; i++)
            {
                GameObject obj = hm.herbivores[i];
                if (obj == null) continue;
                if (!obj.TryGetComponent<CreatureVirtualGauge>(out var gauge)) continue;
                gaugeTargets.Add(gauge);
            }
        }

        if (predatorManager != null && predatorManager.TryGetComponent<predatorManager>(out var pm))
        {
            for (int i = 0; i < pm.predators.Count; i++)
            {
                GameObject obj = pm.predators[i];
                if (obj == null) continue;
                if (!obj.TryGetComponent<CreatureVirtualGauge>(out var gauge)) continue;
                gaugeTargets.Add(gauge);
            }
        }
    }

    void SyncGaugeViews()
    {
        var stale = ListPool<CreatureVirtualGauge>.Get();
        foreach (var pair in gaugeViews)
        {
            if (!gaugeTargets.Contains(pair.Key) || pair.Key == null)
                stale.Add(pair.Key);
        }

        for (int i = 0; i < stale.Count; i++)
            RemoveGaugeView(stale[i]);

        ListPool<CreatureVirtualGauge>.Release(stale);

        for (int i = 0; i < gaugeTargets.Count; i++)
        {
            CreatureVirtualGauge gauge = gaugeTargets[i];
            if (gaugeViews.ContainsKey(gauge))
                continue;

            gaugeViews.Add(gauge, CreateGaugeView(gauge.name));
        }
    }

    GaugeView CreateGaugeView(string gaugeName)
    {
        GameObject rootObj = new GameObject(gaugeName + "_Gauge", typeof(RectTransform));
        rootObj.transform.SetParent(virtualGaugeCanvasRect, false);

        RectTransform root = rootObj.GetComponent<RectTransform>();
        root.sizeDelta = new Vector2(160f, 38f);
        root.pivot = new Vector2(0.5f, 0.5f);

        Slider healthSlider = CreateGaugeSlider("HealthSlider", root, new Vector2(0f, 14f), new Color(1f, 0.52f, 0.12f, 0.95f), out var healthBackground, out var healthFill);
        Slider manaSlider = CreateGaugeSlider("ManaSlider", root, new Vector2(0f, 1f), new Color(1f, 0.87f, 0.16f, 0.95f), out var manaBackground, out var manaFill);
        Slider resourceManaSlider = CreateGaugeSlider("ResourceManaSlider", root, new Vector2(0f, -12f), new Color(0.35f, 0.82f, 1f, 0.95f), out var resourceManaBackground, out var resourceManaFill);
        TextMeshProUGUI manaLapText = CreateGaugeLabel("ManaLapText", root, new Vector2(-86f, 1f));

        rootObj.SetActive(false);

        return new GaugeView
        {
            Root = root,
            HealthSlider = healthSlider,
            ManaSlider = manaSlider,
            ResourceManaSlider = resourceManaSlider,
            ManaLapText = manaLapText,
            HealthBackground = healthBackground,
            ManaBackground = manaBackground,
            ResourceManaBackground = resourceManaBackground,
            HealthFill = healthFill,
            ManaFill = manaFill,
            ResourceManaFill = resourceManaFill,
            HealthBackgroundColor = healthBackground.color,
            ManaBackgroundColor = manaBackground.color,
            ResourceManaBackgroundColor = resourceManaBackground.color,
            HealthFillColor = healthFill.color,
            ManaFillColor = manaFill.color,
            ResourceManaFillColor = resourceManaFill.color,
            ManaLapTextColor = manaLapText.color
        };
    }

    Slider CreateGaugeSlider(string name, RectTransform parent, Vector2 anchoredPosition, Color fillColor, out Image backgroundImage, out Image fillImage)
    {
        GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = anchoredPosition;
        sliderRect.sizeDelta = new Vector2(144f, 10f);

        GameObject backgroundObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        backgroundObj.transform.SetParent(sliderObj.transform, false);
        RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundImage = backgroundObj.GetComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillImage = fillObj.GetComponent<Image>();
        fillImage.color = fillColor;

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.targetGraphic = fillImage;
        slider.fillRect = fillRect;
        slider.handleRect = null;

        return slider;
    }

    TextMeshProUGUI CreateGaugeLabel(string name, RectTransform parent, Vector2 anchoredPosition)
    {
        GameObject labelObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(parent, false);

        RectTransform rect = labelObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(40f, 12f);

        TextMeshProUGUI text = labelObj.GetComponent<TextMeshProUGUI>();
        text.fontSize = 10f;
        text.color = new Color(0.75f, 0.92f, 1f, 0.95f);
        text.alignment = TextAlignmentOptions.MidlineRight;
        text.text = "1x";
        return text;
    }

    void ApplyGaugeAlpha(GaugeView view, float alpha)
    {
        float clampedAlpha = Mathf.Clamp01(alpha);
        SetGraphicAlpha(view.HealthBackground, view.HealthBackgroundColor, clampedAlpha);
        SetGraphicAlpha(view.ManaBackground, view.ManaBackgroundColor, clampedAlpha);
        SetGraphicAlpha(view.ResourceManaBackground, view.ResourceManaBackgroundColor, clampedAlpha);
        SetGraphicAlpha(view.HealthFill, view.HealthFillColor, clampedAlpha);
        SetGraphicAlpha(view.ManaFill, view.ManaFillColor, clampedAlpha);
        SetGraphicAlpha(view.ResourceManaFill, view.ResourceManaFillColor, clampedAlpha);

        if (view.ManaLapText != null)
        {
            Color color = view.ManaLapTextColor;
            color.a *= clampedAlpha;
            view.ManaLapText.color = color;
        }
    }

    void SetGraphicAlpha(Graphic graphic, Color baseColor, float alpha)
    {
        if (graphic == null)
            return;

        Color color = baseColor;
        color.a *= alpha;
        graphic.color = color;
    }


    void RemoveGaugeView(CreatureVirtualGauge gauge)
    {
        if (!gaugeViews.TryGetValue(gauge, out var view))
            return;

        if (view.Root != null)
            Destroy(view.Root.gameObject);

        gaugeViews.Remove(gauge);
    }

    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new();

        public static List<T> Get()
        {
            return pool.Count > 0 ? pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
