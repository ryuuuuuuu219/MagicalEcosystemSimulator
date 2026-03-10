using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public partial class WorldUIManager
{
    List<GameObject> createdUI = new();

    public void ClearObjectList()
    {
        foreach (var ui in createdUI)
        {
            Destroy(ui);
        }

        createdUI.Clear();
    }

    public void BuildObjectList()
    {
        isObjectListVisible = !isObjectListVisible;
        if (isObjectListVisible)
        {
            DisplayObjectList();
        }
        else
        {
            ClearObjectList();
        }
    }

    void DisplayObjectList()
    {
        int categoryNum = 3;

        List<GameObject> grassList = grassManager.GetComponent<ResourceDispenser>().grasses;
        SettingDisplay(0, categoryNum, grassList);

        List<GameObject> herbivoreList = herbivoreManager.GetComponent<herbivoreManager>().herbivores;
        SettingDisplay(1, categoryNum, herbivoreList);

        List<GameObject> predatorList = predatorManager.GetComponent<predatorManager>().predators;
        SettingDisplay(2, categoryNum, predatorList);
    }

    public void SetTarget(GameObject obj)
    {
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

        foreach (var go in StatusUIlist)
        {
            go.SetActive(true);
        }

        UpdateFollowText();

        if (IsStateViewVisible)
        {
            viewDisplayFoundation.SetActive(true);
            Stateview(stateViewPage);
        }
    }

    [Header("button Prefabs")]
    public GameObject buttonPrefab;

    void SettingDisplay(int index, int categorynum, List<GameObject> list)
    {
        float screenWidth = mainCanvas.GetComponent<RectTransform>().rect.width;

        GameObject scrollObj = new GameObject("ScrollView_" + index);
        scrollObj.transform.SetParent(mainCanvas.transform, false);
        createdUI.Add(scrollObj);

        RectTransform rect = scrollObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(index / (float)categorynum, 0);
        rect.anchorMax = new Vector2((index + 1) / (float)categorynum, 1f / 3f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vRect = viewport.AddComponent<RectTransform>();
        vRect.anchorMin = Vector2.zero;
        vRect.anchorMax = Vector2.one;
        vRect.offsetMin = Vector2.zero;
        vRect.offsetMax = Vector2.zero;

        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        Image img = viewport.AddComponent<Image>();

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform cRect = content.AddComponent<RectTransform>();
        cRect.anchorMin = new Vector2(0, 1);
        cRect.anchorMax = new Vector2(1, 1);
        cRect.pivot = new Vector2(0.5f, 1);

        cRect.anchoredPosition = Vector2.zero;
        cRect.sizeDelta = new Vector2(0, 1);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.horizontal = false;
        scrollRect.viewport = vRect;
        scrollRect.content = cRect;

        LayoutElement layoutElement;
        if (!buttonPrefab.TryGetComponent<LayoutElement>(out layoutElement))
        {
            layoutElement = buttonPrefab.AddComponent<LayoutElement>();
        }

        layoutElement.preferredHeight = 20;

        foreach (GameObject obj in list)
        {
            if (obj == null) continue;

            GameObject btn = Instantiate(buttonPrefab, content.transform);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = obj.name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                SetTarget(obj);
            });
        }
    }
}
