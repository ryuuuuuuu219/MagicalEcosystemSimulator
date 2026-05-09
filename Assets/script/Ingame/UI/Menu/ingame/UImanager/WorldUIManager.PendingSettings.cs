using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class WorldUIManager
{
    enum PendingSettingsView
    {
        All,
        GenerationOnly
    }

    GameObject pendingSettingsPanelRoot;
    Transform pendingSettingsContentRoot;
    TextMeshProUGUI pendingSettingsTitleText;
    LayerMask defaultSelectableLayer;
    bool hasDefaultSelectableLayer;
    PendingSettingsView pendingSettingsView = PendingSettingsView.All;
    bool pendingSettingsReturnsToGenerationBranch;

    void EnsurePendingSettingsPanel()
    {
        EnsurePendingUiPropertySources();

        if (!hasDefaultSelectableLayer)
        {
            defaultSelectableLayer = selectableLayer;
            hasDefaultSelectableLayer = true;
        }

        if (pendingSettingsPanelRoot != null)
            return;

        if (mainCanvas == null)
            mainCanvas = FindFirstObjectByType<Canvas>();

        if (mainCanvas == null)
            return;

        pendingSettingsPanelRoot = new GameObject("PendingSettingsPanel", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        pendingSettingsPanelRoot.transform.SetParent(mainCanvas.transform, false);
        pendingSettingsPanelRoot.SetActive(false);

        RectTransform rootRect = pendingSettingsPanelRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.22f, 0.05f);
        rootRect.anchorMax = new Vector2(0.98f, 0.96f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image bg = pendingSettingsPanelRoot.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.72f);

        pendingSettingsPanelRoot.GetComponent<LayoutElement>().ignoreLayout = true;

        pendingSettingsTitleText = CreateSettingsLabel(
            pendingSettingsPanelRoot.transform,
            "PendingSettingsTitle",
            "Pending Settings",
            20f,
            TextAlignmentOptions.MidlineLeft);
        RectTransform titleRect = pendingSettingsTitleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.02f, 0.93f);
        titleRect.anchorMax = new Vector2(0.82f, 0.99f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Button closeButton = CreateSettingsButton(pendingSettingsPanelRoot.transform, "Close", ClosePendingSettingsPanel);
        closeButton.name = "PendingSettingsCloseButton";
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.84f, 0.935f);
        closeRect.anchorMax = new Vector2(0.98f, 0.99f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;

        GameObject scrollObj = new GameObject("PendingSettingsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObj.transform.SetParent(pendingSettingsPanelRoot.transform, false);
        RectTransform scrollRectTransform = scrollObj.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.02f, 0.02f);
        scrollRectTransform.anchorMax = new Vector2(0.98f, 0.92f);
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;
        scrollObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 6f;
        layout.padding = new RectOffset(8, 8, 8, 8);

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 24f;

        pendingSettingsContentRoot = content.transform;
        RebuildPendingSettingsContent();
    }

    void SetPendingSettingsVisible(bool visible)
    {
        EnsurePendingSettingsPanel();
        if (pendingSettingsPanelRoot == null)
            return;

        if (visible)
            RebuildPendingSettingsContent();

        pendingSettingsPanelRoot.SetActive(visible);
    }

    void OpenAllPendingSettingsPanel()
    {
        pendingSettingsView = PendingSettingsView.All;
        pendingSettingsReturnsToGenerationBranch = false;
        SetPendingSettingsVisible(true);
    }

    void OpenGenerationSettingsPanel()
    {
        pendingSettingsView = PendingSettingsView.GenerationOnly;
        pendingSettingsReturnsToGenerationBranch = true;
        SetPendingSettingsVisible(true);
    }

    void ClosePendingSettingsPanel()
    {
        SetPendingSettingsVisible(false);

        if (pendingSettingsReturnsToGenerationBranch)
            PushBranch(generationBranch);
        else
            PushBranch(menuRootBranch);

        pendingSettingsReturnsToGenerationBranch = false;
        pendingSettingsView = PendingSettingsView.All;
    }
}
