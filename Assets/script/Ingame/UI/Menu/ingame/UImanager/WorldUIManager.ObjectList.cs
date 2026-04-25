using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public partial class WorldUIManager
{
    List<GameObject> createdUI = new();
    category currentObjectListCategory = category.grass;
    int runtimeObjectSelectId;

    public void ClearObjectList()
    {
        foreach (var ui in createdUI)
        {
            if (ui != null)
                Destroy(ui);
        }

        createdUI.Clear();
    }

    public void BuildObjectList()
    {
        isObjectListVisible = !isObjectListVisible;
        if (isObjectListVisible)
        {
            RefreshObjectSources();
            DisplayObjectList();
        }
        else
            ClearObjectList();
    }

    public void RefreshObjectSources()
    {
        if (grassManager != null && grassManager.TryGetComponent<ResourceDispenser>(out var dispenser))
        {
            dispenser.grasses = new List<GameObject>();
            Resource[] resources = FindObjectsByType<Resource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var resource in resources)
            {
                if (resource == null || resource.resourceCategory != category.grass)
                    continue;

                dispenser.grasses.Add(resource.gameObject);
            }
        }

        if (herbivoreManager != null && herbivoreManager.TryGetComponent<herbivoreManager>(out var herbivoreMgr))
        {
            herbivoreMgr.herbivores = new List<GameObject>();
            herbivoreBehaviour[] herbivores = FindObjectsByType<herbivoreBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var herbivore in herbivores)
            {
                if (herbivore == null || herbivore.gameObject == null)
                    continue;

                herbivoreMgr.herbivores.Add(herbivore.gameObject);
            }
        }

        if (predatorManager != null && predatorManager.TryGetComponent<predatorManager>(out var predatorMgr))
        {
            predatorMgr.predators = new List<GameObject>();
            predatorBehaviour[] predators = FindObjectsByType<predatorBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var predator in predators)
            {
                if (predator == null || predator.gameObject == null)
                    continue;

                predatorMgr.predators.Add(predator.gameObject);
            }
        }
    }

    void DisplayObjectList()
    {
        ClearObjectList();
        runtimeObjectSelectId = 0;

        GameObject root = CreateObjectListRoot(mainCanvas.transform);
        Transform content = CreateObjectListContent(root.transform);
        CreateCategorySwitch(root.transform);

        switch (currentObjectListCategory)
        {
            case category.herbivore:
                SettingDisplay("Herbivore", herbivoreManager.GetComponent<herbivoreManager>().herbivores, content);
                break;
            case category.predator:
                SettingDisplay("Predator", predatorManager.GetComponent<predatorManager>().predators, content);
                break;
            default:
                SettingDisplay("Grass", grassManager.GetComponent<ResourceDispenser>().grasses, content);
                break;
        }
    }

    GameObject CreateObjectListRoot(Transform parent)
    {
        GameObject root = new GameObject("ObjectListRoot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        createdUI.Add(root);

        RectTransform rect = root.GetComponent<RectTransform>();
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-16f, 16f);
        rect.sizeDelta = new Vector2(canvasRect.rect.width * 0.75f, canvasRect.rect.height * (2f / 3f));

        Image bg = root.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        LayoutElement layout = root.GetComponent<LayoutElement>();
        layout.ignoreLayout = true;
        return root;
    }

    void CreateCategorySwitch(Transform parent)
    {
        GameObject header = new GameObject("CategorySwitch", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        header.transform.SetParent(parent, false);

        RectTransform rect = header.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -6f);
        rect.offsetMin = new Vector2(6f, 0f);
        rect.offsetMax = new Vector2(-6f, 0f);
        rect.sizeDelta = new Vector2(0f, 30f);

        LayoutElement layout = header.GetComponent<LayoutElement>();
        layout.preferredHeight = 30f;

        HorizontalLayoutGroup group = header.GetComponent<HorizontalLayoutGroup>();
        group.childControlWidth = false;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = false;
        group.spacing = 6f;
        group.padding = new RectOffset(4, 4, 4, 4);

        CreateControlButton(header.transform, "<", () =>
        {
            currentObjectListCategory = PreviousCategory(currentObjectListCategory);
            DisplayObjectList();
        }, "StateViewPageDown_leaf_00x1");

        GameObject labelRoot = new GameObject("CategoryLabelRoot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        labelRoot.transform.SetParent(header.transform, false);
        labelRoot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);
        LayoutElement labelLayout = labelRoot.GetComponent<LayoutElement>();
        labelLayout.preferredWidth = 180f;
        labelLayout.preferredHeight = 22f;
        CreateChildLabel(labelRoot.transform, currentObjectListCategory.ToString(), 16, TextAlignmentOptions.Center);

        CreateControlButton(header.transform, ">", () =>
        {
            currentObjectListCategory = NextCategory(currentObjectListCategory);
            DisplayObjectList();
        }, "StateViewPageUp_leaf_00x2");
    }

    category NextCategory(category value)
    {
        switch (value)
        {
            case category.grass:
                return category.herbivore;
            case category.herbivore:
                return category.predator;
            default:
                return category.grass;
        }
    }

    category PreviousCategory(category value)
    {
        switch (value)
        {
            case category.predator:
                return category.herbivore;
            case category.herbivore:
                return category.grass;
            default:
                return category.predator;
        }
    }

    Transform CreateObjectListContent(Transform parent)
    {
        GameObject scrollObj = new GameObject("ObjectListScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObj.transform.SetParent(parent, false);

        RectTransform rect = scrollObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(6f, 6f);
        rect.offsetMax = new Vector2(-6f, -42f);

        scrollObj.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

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
        layout.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        return content.transform;
    }

    public void SetTarget(GameObject obj)
    {
        if (obj == null)
        {
            if (currentTarget == null)
                UpdateFollowText();
            return;
        }

        currentTarget = obj;

        if (RotationThenlooking)
        {
            Vector3 currentPos = mainCamera.transform.rotation * Vector3.forward * 20f;
            mainCamera.transform.position = currentTarget.transform.position - currentPos;
        }
        else
        {
            mainCamera.transform.position = obj.transform.position + new Vector3(0, 20, 0);
            mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        var freeFly = mainCamera.GetComponent<FreeFlyCamera>();
        if (freeFly != null)
            freeFly.SyncRotationFromTransform();

        ShowStatusButtons();

        UpdateFollowText();

        if (IsStateViewVisible)
        {
            viewDisplayFoundation.SetActive(true);
            Stateview(stateViewPage);
        }
    }

    void ShowStatusButtons()
    {
        foreach (GameObject go in EnumerateStatusButtons())
        {
            if (go == null) continue;
            go.SetActive(true);
            go.transform.SetAsLastSibling();
        }

        Debug.Log($"[WorldUI] ShowStatusButtons target={currentTarget?.name ?? "null"} objectListVisible={isObjectListVisible} statusVisible={isStatusVisible}");
    }

    [Header("button Prefabs")]
    public GameObject buttonPrefab;

    void SettingDisplay(string categoryName, List<GameObject> list, Transform parent)
    {
        GameObject categoryRoot = new GameObject(categoryName + "_Category", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        categoryRoot.transform.SetParent(parent, false);
        createdUI.Add(categoryRoot);

        Image bg = categoryRoot.GetComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.03f);

        VerticalLayoutGroup layout = categoryRoot.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 4f;
        layout.padding = new RectOffset(6, 6, 6, 6);

        ContentSizeFitter fitter = categoryRoot.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        CreateCategoryHeader(categoryRoot.transform, categoryName, list != null ? list.Count : 0);

        if (list == null)
            return;

        foreach (GameObject obj in list)
        {
            if (obj == null) continue;
            CreateObjectEntry(categoryRoot.transform, obj);
        }
    }

    void CreateCategoryHeader(Transform parent, string categoryName, int count)
    {
        GameObject header = new GameObject(categoryName + "_Header", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        header.transform.SetParent(parent, false);

        Image image = header.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.08f);

        LayoutElement layout = header.GetComponent<LayoutElement>();
        layout.preferredHeight = 24f;

        TextMeshProUGUI label = CreateChildLabel(header.transform, $"{categoryName} ({count})", 16, TextAlignmentOptions.MidlineLeft);
        RectTransform rect = label.GetComponent<RectTransform>();
        rect.offsetMin = new Vector2(8f, 0f);
        rect.offsetMax = new Vector2(-8f, 0f);
    }

    void CreateObjectEntry(Transform parent, GameObject obj)
    {
        GameObject entry = new GameObject(obj.name + "_Entry", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        entry.transform.SetParent(parent, false);
        createdUI.Add(entry);

        Image bg = entry.GetComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.02f);

        VerticalLayoutGroup layout = entry.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 2f;
        layout.padding = new RectOffset(4, 4, 4, 4);

        ContentSizeFitter fitter = entry.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject mainButton = Instantiate(buttonPrefab, entry.transform);
        mainButton.name = $"ObjectSelect_branch_00{ToRuntimeBranchId(runtimeObjectSelectId)}";
        runtimeObjectSelectId++;
        TextMeshProUGUI label = mainButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = obj.name;

        Button button = mainButton.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (obj == null)
            {
                createdUI.Remove(entry);
                Destroy(entry);
                return;
            }

            SetTarget(obj);
        });
    }

    void CreateControlButton(Transform parent, string text, UnityEngine.Events.UnityAction action, string objectName)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, parent);
        buttonObj.name = objectName;

        LayoutElement layout = buttonObj.GetComponent<LayoutElement>();
        if (layout == null)
            layout = buttonObj.AddComponent<LayoutElement>();

        layout.preferredHeight = 20f;
        layout.preferredWidth = text == "detail" ? 80f : 36f;
        layout.flexibleWidth = text == "detail" ? 1f : 0f;

        TextMeshProUGUI label = buttonObj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.text = text;

        Button button = buttonObj.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    static string ToRuntimeBranchId(int value)
    {
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        if (value < 0)
            value = 0;

        if (value < alphabet.Length)
            return alphabet[value].ToString();

        int high = value / alphabet.Length;
        int low = value % alphabet.Length;
        if (high >= alphabet.Length)
            high = alphabet.Length - 1;

        return $"{alphabet[high]}{alphabet[low]}";
    }

    TextMeshProUGUI CreateChildLabel(Transform parent, string textValue, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        return text;
    }
}
