using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class WorldUIManager
{
    [Header("Virtual Gauge")]
    [SerializeField] bool showVirtualGauges = true;
    [SerializeField] float virtualGaugePlaneDistance = 2.5f;
    [SerializeField] float selectedGaugeScale = 1.3f;

    Canvas virtualGaugeCanvas;
    RectTransform virtualGaugeCanvasRect;
    readonly Dictionary<CreatureVirtualGauge, GaugeView> gaugeViews = new();
    readonly List<CreatureVirtualGauge> gaugeTargets = new();

    sealed class GaugeView
    {
        public RectTransform Root;
        public Image HealthFill;
        public Image EnergyFill;
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

    void UpdateVirtualGaugeVisibility()
    {
        if (virtualGaugeCanvas != null)
            virtualGaugeCanvas.enabled = showVirtualGauges;
    }

    void UpdateVirtualGauges()
    {
        InitializeVirtualGaugeCanvas();
        UpdateVirtualGaugeVisibility();

        if (!showVirtualGauges || mainCamera == null || virtualGaugeCanvasRect == null)
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

            if (!gauge.TryGetRatios(out float healthRatio, out float energyRatio))
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
            view.HealthFill.fillAmount = healthRatio;
            view.EnergyFill.fillAmount = energyRatio;
        }
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
        root.sizeDelta = new Vector2(160f, 24f);

        GameObject healthBgObj = CreateGaugeBar("HealthBg", root, new Vector2(0f, 12f));
        Image healthBg = healthBgObj.GetComponent<Image>();
        healthBg.color = new Color(0f, 0f, 0f, 0.45f);
        Image healthFill = CreateFill("HealthFill", healthBgObj.transform, new Color(1f, 0.52f, 0.12f, 0.95f));

        GameObject energyBgObj = CreateGaugeBar("EnergyBg", root, new Vector2(0f, -2f));
        Image energyBg = energyBgObj.GetComponent<Image>();
        energyBg.color = new Color(0f, 0f, 0f, 0.45f);
        Image energyFill = CreateFill("EnergyFill", energyBgObj.transform, new Color(1f, 0.87f, 0.16f, 0.95f));

        rootObj.SetActive(false);

        return new GaugeView
        {
            Root = root,
            HealthFill = healthFill,
            EnergyFill = energyFill
        };
    }

    GameObject CreateGaugeBar(string name, RectTransform parent, Vector2 anchoredPosition)
    {
        GameObject barObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        barObj.transform.SetParent(parent, false);

        RectTransform rect = barObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(144f, 10f);
        return barObj;
    }

    Image CreateFill(string name, Transform parent, Color color)
    {
        GameObject fillObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(parent, false);

        RectTransform rect = fillObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = fillObj.GetComponent<Image>();
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
        image.color = color;
        image.fillAmount = 1f;
        return image;
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
