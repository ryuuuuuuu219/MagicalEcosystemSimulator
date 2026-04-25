using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class WorldUIManager : MonoBehaviour
{
    void Awake()
    {
        EnsurePendingUiPropertySources();
        EnsureSceneButtonBindings();
    }

    void OnEnable()
    {
        EnsureSceneButtonBindings();
    }

    void Update()
    {
        UpdateVirtualGauges();

        if (currentTarget != null)
        {
            Vector3 currentPos = mainCamera.transform.rotation * Vector3.forward * 20f;
            mainCamera.transform.position = currentTarget.transform.position - currentPos;
            UpdateFollowText();
        }
        else if (text_f != null)
        {
            text_f.text = string.Empty;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Objectpic();

            UpdateFollowText();
        }
    }

    private void Start()
    {
        EnsurePendingUiPropertySources();
        EnsureSceneButtonBindings();
        RectTransform rect = waveImage.GetComponent<RectTransform>();
        InitWaveTexture((int)rect.rect.width, (int)rect.rect.height);
        ClearTextureTransparent();
        text_f = text_follow.GetComponent<TextMeshProUGUI>();

        InitializeVirtualGaugeManager();
        InitializeVirtualGaugeCanvas();
        UpdateVirtualGaugeVisibility();
        InitializeMenuBranchVisibility();

        foreach (GameObject go in EnumerateInitialUiObjects())
        {
            if (go == null) continue;
            go.SetActive(false);
        }
    }

    void Objectpic()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f, selectableLayer);
        if (hits.Length > 0)
        {
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            GameObject target = hits[0].collider.gameObject;
            if (target == currentTarget && IsCreature(target))
            {
                for (int i = 1; i < hits.Length; i++)
                {
                    GameObject alternate = hits[i].collider.gameObject;
                    if (alternate != currentTarget && IsCreature(alternate))
                    {
                        target = alternate;
                        break;
                    }
                }
            }

            SetTarget(target);
            if (!isWorldMenuVisible)
            {
                isWorldMenuVisible = true;
                ShowMenuRootButtons();
            }
        }
    }

    bool IsCreature(GameObject obj)
    {
        if (obj == null)
            return false;

        return obj.TryGetComponent<herbivoreBehaviour>(out _) ||
               obj.TryGetComponent<predatorBehaviour>(out _);
    }

    /// <summary>
    /// ワールドメニュー全体の表示状態を切り替えます。
    /// 閉じる際は配下 UI と選択状態もまとめてリセットします。
    /// </summary>
    /// <seealso cref="ShowMenuRootButtons"/>
    /// <seealso cref="HideAllMenuBranches"/>
    public void Menu()
    {
        if (lastMenuInvokeFrame == Time.frameCount)
            return;

        lastMenuInvokeFrame = Time.frameCount;
        isWorldMenuVisible = !isWorldMenuVisible;
        if (isWorldMenuVisible)
        {
            ShowMenuRootButtons();
        }
        else
        {
            isObjectListVisible = false;
            isStatusVisible = false;

            HideAllMenuBranches();
            currentTarget = null;
            ClearObjectList();
            HideStatusUI();
            ClearStateview();
            UpdateFollowText();
        }
    }

    /// <summary>
    /// オブジェクト一覧ブランチの表示を切り替えます。
    /// </summary>
    /// <seealso cref="ToggleObjectListBranch"/>
    public void Onclickbutton1()
    {
        if (IsDuplicateUiInvoke(ref lastObjectListInvokeFrame, nameof(Onclickbutton1)))
            return;

        ToggleObjectListBranch();
    }

    /// <summary>
    /// 生成メニューブランチの表示を切り替えます。
    /// </summary>
    /// <seealso cref="ToggleGenerationBranch"/>
    public void Onclickbutton2()
    {
        if (IsDuplicateUiInvoke(ref lastGenerationInvokeFrame, nameof(Onclickbutton2)))
            return;

        ToggleGenerationBranch();
    }

    /// <summary>
    /// AdvanceGeneration パネルを開いて世代更新処理を実行します。
    /// </summary>
    /// <seealso cref="OpenAdvanceGenerationBranch"/>
    public void Onclickbutton2_1()
    {
        if (IsDuplicateUiInvoke(ref lastAdvanceGenerationInvokeFrame, nameof(Onclickbutton2_1)))
            return;

        OpenAdvanceGenerationBranch();
    }

    /// <summary>
    /// Genome Viewer パネルの表示を切り替えます。
    /// </summary>
    /// <seealso cref="ToggleGenomeViewerBranch"/>
    public void Onclickbutton2_2()
    {
        if (IsDuplicateUiInvoke(ref lastGenomeViewerInvokeFrame, nameof(Onclickbutton2_2)))
            return;

        ToggleGenomeViewerBranch();
    }

    /// <summary>
    /// Genome Injector パネルの表示を切り替えます。
    /// </summary>
    /// <seealso cref="ToggleGenomeInjectorBranch"/>
    public void Onclickbutton2_3()
    {
        if (IsDuplicateUiInvoke(ref lastGenomeInjectorInvokeFrame, nameof(Onclickbutton2_3)))
            return;

        ToggleGenomeInjectorBranch();
    }

    public void Onclickbutton3()
    {
        if (IsDuplicateUiInvoke(ref lastPropertiesInvokeFrame, nameof(Onclickbutton3)))
            return;

        TogglePropertiesBranch();
    }

    void UpdateFollowText()
    {
        if (text_f == null)
            return;

        if (currentTarget == null)
        {
            text_f.text = string.Empty;
            return;
        }

        if (isStatusVisible)
            return;

        string statusText = "follow:" + currentTarget.name;

        if (currentTarget.TryGetComponent<Resource>(out var resource))
        {
            statusText += "\ncarbon:" + resource.carbon.ToString("F1");
            statusText += "\nmaxCarbon:" + resource.maxCarbon.ToString("F1");
            statusText += "\ncategory:" + resource.resourceCategory;
        }

        if (currentTarget.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            statusText += "\nhealth:" + herbivore.health.ToString("F1");
            statusText += "\nenergy:" + herbivore.energy.ToString("F1");
            statusText += "\ndead:" + herbivore.IsDead;
        }
        else if (currentTarget.TryGetComponent<predatorBehaviour>(out var predator))
        {
            statusText += "\nhealth:" + predator.health.ToString("F1");
            statusText += "\nenergy:" + predator.energy.ToString("F1");
            statusText += "\ndead:" + predator.IsDead;
        }

        text_f.text = statusText;
    }

    void EnsureSceneButtonBindings()
    {
        menuRootButton = GetButton(menuRootBranch);

        EnsureMenuButtonRelay(menuRootButton);
        BindSceneButton(GetObjectListButton(), Onclickbutton1);
        BindSceneButton(GetGenerationButton(), Onclickbutton2);
        BindSceneButton(GetStateButton(), Onclick_State);
        BindSceneButton(GetPageDownButton(), Onclick_PageDown);
        BindSceneButton(GetPageUpButton(), Onclick_PageUp);
        BindSceneButton(GetAdvanceGenerationButton(), Onclickbutton2_1);
        BindSceneButton(GetGenomeViewerButton(), Onclickbutton2_2);
        BindSceneButton(GetGenomeInjectorButton(), Onclickbutton2_3);
        BindSceneButton(GetPropertiesButton(), Onclickbutton3);
    }

    Button GetObjectListButton() => GetButton(objectListBranch);

    Button GetGenerationButton() => GetButton(generationBranch);

    Button GetPropertiesButton() => GetButton(propertiesBranch);

    Button GetStateButton() => GetButton(detailBranch);

    Button GetPageDownButton() => GetButton(stateViewPageDownBranch);

    Button GetPageUpButton() => GetButton(stateViewPageUpBranch);

    Button GetAdvanceGenerationButton() => GetButton(advanceGenerationBranch);

    Button GetGenomeViewerButton() => GetButton(genomeViewerBranch);

    Button GetGenomeInjectorButton() => GetButton(genomeInjectorBranch);

    Button GetButton(GameObject go)
    {
        return go != null ? go.GetComponent<Button>() : null;
    }

    Button GetButton(UITreeBranch branch)
    {
        return branch != null ? branch.GetComponent<Button>() : null;
    }

    IEnumerable<GameObject> EnumerateInitialUiObjects()
    {
        yield return GetBranchObject(objectListBranch);
        yield return GetBranchObject(generationBranch);
        yield return GetBranchObject(propertiesBranch);
        yield return GetBranchObject(detailBranch);
        yield return GetBranchObject(stateViewPageDownBranch);
        yield return GetBranchObject(stateViewPageUpBranch);
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
        yield return GetBranchObject(genomeViewerBranch);
        yield return GetBranchObject(genomeInjectorBranch);
        yield return GetBranchObject(advanceGenerationBranch);
    }

    IEnumerable<GameObject> EnumerateStatusButtons()
    {
        yield return GetBranchObject(detailBranch);
        yield return GetBranchObject(stateViewPageDownBranch);
        yield return GetBranchObject(stateViewPageUpBranch);
    }

    IEnumerable<GameObject> EnumerateStatusInfoObjects()
    {
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
    }

    IEnumerable<GameObject> EnumerateGenerationButtons()
    {
        yield return GetBranchObject(genomeViewerBranch);
        yield return GetBranchObject(genomeInjectorBranch);
        yield return GetBranchObject(advanceGenerationBranch);
    }

    static GameObject GetBranchObject(UITreeBranch branch)
    {
        return branch != null ? branch.gameObject : null;
    }

    static void BindSceneButton(Button button, UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void EnsureMenuButtonRelay(Button button)
    {
        if (button == null)
            return;

        if (!button.TryGetComponent<WorldMenuButtonRelay>(out var relay))
            relay = button.gameObject.AddComponent<WorldMenuButtonRelay>();

        relay.Bind(this);
    }

    bool IsDuplicateUiInvoke(ref int lastInvokeFrame, string actionName)
    {
        if (lastInvokeFrame == Time.frameCount)
            return true;

        lastInvokeFrame = Time.frameCount;
        return false;
    }
}

public sealed class WorldMenuButtonRelay : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    WorldUIManager manager;

    public void Bind(WorldUIManager target)
    {
        manager = target;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        manager?.Menu();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        manager?.Menu();
    }
}
