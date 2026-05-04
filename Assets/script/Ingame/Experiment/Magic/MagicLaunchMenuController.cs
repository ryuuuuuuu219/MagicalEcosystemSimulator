using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MagicLaunchMenuController : MonoBehaviour
{
    public MagicMaterialExperimentLauncher launcher;

    TMP_Dropdown elementDropdown;

    void Awake()
    {
        EnsureLauncher();
        BuildUI();
        ApplySelection(elementDropdown != null ? elementDropdown.value : 1);
    }

    void OnEnable()
    {
        EnsureLauncher();
    }

    void EnsureLauncher()
    {
        if (launcher == null)
            launcher = GetComponent<MagicMaterialExperimentLauncher>();
        if (launcher == null)
            launcher = gameObject.AddComponent<MagicMaterialExperimentLauncher>();
        if (launcher.sourceCamera == null)
            launcher.sourceCamera = Camera.main;
    }

    void BuildUI()
    {
        if (elementDropdown != null)
            return;

        GameObject panel = new GameObject("MagicLaunchPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(320f, 132f);
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

        TextMeshProUGUI title = CreateText("Title", panel.transform, "projectile element", 16f, TextAlignmentOptions.MidlineLeft);
        title.rectTransform.anchorMin = new Vector2(0f, 1f);
        title.rectTransform.anchorMax = new Vector2(1f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);
        title.rectTransform.anchoredPosition = new Vector2(0f, -8f);
        title.rectTransform.offsetMin = new Vector2(16f, title.rectTransform.offsetMin.y);
        title.rectTransform.offsetMax = new Vector2(-16f, title.rectTransform.offsetMax.y);
        title.rectTransform.sizeDelta = new Vector2(0f, 28f);

        elementDropdown = CreateDropdown(panel.transform);
        elementDropdown.onValueChanged.AddListener(ApplySelection);
    }

    TMP_Dropdown CreateDropdown(Transform parent)
    {
        GameObject root = new GameObject("MagicElementDropdown", typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
        root.transform.SetParent(parent, false);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(16f, -48f);
        rect.sizeDelta = new Vector2(260f, 42f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.08f, 0.9f);

        TextMeshProUGUI label = CreateText("Label", root.transform, "ice", 18f, TextAlignmentOptions.MidlineLeft);
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
        TMP_Dropdown dropdown = root.GetComponent<TMP_Dropdown>();
        dropdown.targetGraphic = background;
        dropdown.captionText = label;
        dropdown.itemText = template.GetComponentInChildren<Toggle>(true).GetComponentInChildren<TextMeshProUGUI>(true);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.options = new List<TMP_Dropdown.OptionData>
        {
            new("fire"),
            new("ice"),
            new("lightning"),
            new("wind"),
            new("space")
        };
        dropdown.value = 1;
        template.SetActive(false);
        return dropdown;
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
        rect.sizeDelta = new Vector2(0f, 204f);
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
        contentRect.sizeDelta = new Vector2(0f, 202f);

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

    void ApplySelection(int value)
    {
        EnsureLauncher();
        launcher.launchElement = value switch
        {
            0 => MagicElement.Fire,
            2 => MagicElement.Lightning,
            3 => MagicElement.Wind,
            4 => MagicElement.Space,
            _ => MagicElement.Ice
        };
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
}
