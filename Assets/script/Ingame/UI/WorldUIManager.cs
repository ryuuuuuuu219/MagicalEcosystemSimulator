using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class WorldUIManager : MonoBehaviour
{
    [Header("Debug Toggle")]
    [FormerlySerializedAs("showWorldMenu")]
    [SerializeField] bool isWorldMenuVisible = false;
    [FormerlySerializedAs("showObjectList")]
    [SerializeField] bool isObjectListVisible = false;
    [FormerlySerializedAs("showStatus")]
    [SerializeField] bool isStatusVisible = false;

    [Header("UI Branch References")]
    /// <summary>
    /// UITreeBranch に対応する SerializeField 命名規則の正式仕様。
    /// 形式は「機能_階層_ID」。
    /// ID の桁数は階層数を表し、各桁の値はその階層での通し番号 ID を表す。
    /// 9 を超える値が必要な場合は 0-9 の後に A-Z を使って拡張する。
    /// root は最上位階層を表す固定名として扱う。
    /// 固定ボタンの正式名は Menu_root_0, ObjectList_tab_00, GenController_tab_01。
    /// </summary>
    [SerializeField] GameObject Menu_root_0;
    [SerializeField] GameObject ObjectList_tab_00;
    [SerializeField] GameObject GenController_tab_01;
    [SerializeField] GameObject Properties_tab_02;
    /// <summary>
    /// StateView は ObjectList 配下として扱う。
    /// 動的生成ボタンには実行時 ID を振る。
    /// ObjectList 配下の正式名は ObjectSelect_branch_00x, Detail_leaf_00x0,
    /// StateViewPageDown_leaf_00x1, StateViewPageUp_leaf_00x2。
    /// </summary>
    [SerializeField] GameObject Detail_leaf_00x0;
    [SerializeField] GameObject StateViewPageDown_leaf_00x1;
    [SerializeField] GameObject StateViewPageUp_leaf_00x2;
    [SerializeField] GameObject viewDisplayFoundationRoot;
    [SerializeField] GameObject uiFreqRoot;
    /// <summary>
    /// GenController 配下の正式名は LogView_branch_010, GenomeInjector_branch_011,
    /// AdvanceGeneration_branch_012。
    /// </summary>
    [SerializeField] GameObject LogView_branch_010;
    [SerializeField] GameObject GenomeInjector_branch_011;
    [SerializeField] GameObject AdvanceGeneration_branch_012;

    [Header("References")]
    public Canvas mainCanvas;
    public Camera mainCamera;

    [Header("UI")]
    [SerializeField] GameObject text_follow;
    TextMeshProUGUI text_f;

    [Header("Manager State")]
    public GameObject grassManager;
    public GameObject herbivoreManager;
    public GameObject predatorManager;
    public AdvanceGenerationController generationController;

    GameObject currentTarget;
    public bool RotationThenlooking = false;

    [SerializeField] LayerMask selectableLayer;

    [SerializeField] GameObject viewDisplayFoundation;
    [SerializeField] UnityEngine.UI.RawImage waveImage;
    [SerializeField] TextMeshProUGUI UI_freq;

    Texture2D waveTexture;
    Color32[] pixelBuffer;
    int stateViewPage = 0;
    const int stateViewPageCount = 4;
    string currentHerbivoreDnaCode = string.Empty;
    Button herbivoreDnaCopyButton;
    Button menuRootButton;
    int lastMenuInvokeFrame = -1;
    int lastObjectListInvokeFrame = -1;
    int lastGenerationInvokeFrame = -1;
    int lastAdvanceGenerationInvokeFrame = -1;
    int lastGenomeViewerInvokeFrame = -1;
    int lastGenomeInjectorInvokeFrame = -1;
    int lastPropertiesInvokeFrame = -1;
    int lastStateInvokeFrame = -1;
    int lastPageDownInvokeFrame = -1;
    int lastPageUpInvokeFrame = -1;
    bool IsStateViewVisible => isStatusVisible && isObjectListVisible;

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
        menuRootButton = GetButton(Menu_root_0);

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

    Button GetObjectListButton() => GetButton(ObjectList_tab_00);

    Button GetGenerationButton() => GetButton(GenController_tab_01);

    Button GetPropertiesButton() => GetButton(Properties_tab_02);

    Button GetStateButton() => GetButton(Detail_leaf_00x0);

    Button GetPageDownButton() => GetButton(StateViewPageDown_leaf_00x1);

    Button GetPageUpButton() => GetButton(StateViewPageUp_leaf_00x2);

    Button GetAdvanceGenerationButton() => GetButton(AdvanceGeneration_branch_012);

    Button GetGenomeViewerButton() => GetButton(LogView_branch_010);

    Button GetGenomeInjectorButton() => GetButton(GenomeInjector_branch_011);

    Button GetButton(GameObject go)
    {
        return go != null ? go.GetComponent<Button>() : null;
    }

    IEnumerable<GameObject> EnumerateInitialUiObjects()
    {
        yield return ObjectList_tab_00;
        yield return GenController_tab_01;
        yield return Properties_tab_02;
        yield return Detail_leaf_00x0;
        yield return StateViewPageDown_leaf_00x1;
        yield return StateViewPageUp_leaf_00x2;
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
        yield return LogView_branch_010;
        yield return GenomeInjector_branch_011;
        yield return AdvanceGeneration_branch_012;
    }

    IEnumerable<GameObject> EnumerateStatusButtons()
    {
        yield return Detail_leaf_00x0;
        yield return StateViewPageDown_leaf_00x1;
        yield return StateViewPageUp_leaf_00x2;
    }

    IEnumerable<GameObject> EnumerateStatusInfoObjects()
    {
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
    }

    IEnumerable<GameObject> EnumerateGenerationButtons()
    {
        yield return LogView_branch_010;
        yield return GenomeInjector_branch_011;
        yield return AdvanceGeneration_branch_012;
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
