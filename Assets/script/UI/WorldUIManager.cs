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
    [SerializeField] List<GameObject> TabUIlist = new();
    [FormerlySerializedAs("showObjectList")]
    [SerializeField] bool isObjectListVisible = false;
    [FormerlySerializedAs("showStatus")]
    [SerializeField] bool isStatusVisible = false;
    [SerializeField] List<GameObject> StatusUIlist = new();
    [SerializeField] List<GameObject> StatusinfoUIlist = new();

    [SerializeField] List<GameObject> GencontrollerUIlist = new();
    List<List<GameObject>> UIlist = new();

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
    bool IsStateViewVisible => isStatusVisible && isObjectListVisible;

    void Awake()
    {
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
        EnsureSceneButtonBindings();

        UIlist = new() { GencontrollerUIlist, StatusUIlist, StatusinfoUIlist, TabUIlist };
        RectTransform rect = waveImage.GetComponent<RectTransform>();
        InitWaveTexture((int)rect.rect.width, (int)rect.rect.height);
        ClearTextureTransparent();
        text_f = text_follow.GetComponent<TextMeshProUGUI>();

        if (viewDisplayFoundation != null && !StatusinfoUIlist.Contains(viewDisplayFoundation))
            StatusinfoUIlist.Add(viewDisplayFoundation);

        if (UI_freq != null && !StatusinfoUIlist.Contains(UI_freq.gameObject))
            StatusinfoUIlist.Add(UI_freq.gameObject);

        InitializeVirtualGaugeManager();
        InitializeVirtualGaugeCanvas();
        UpdateVirtualGaugeVisibility();

        foreach (var c in UIlist)
        {
            foreach (var go in c)
            {
                if (go == null) continue;
                go.SetActive(false);
            }
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
        {
            Debug.Log($"[WorldUIManager] Menu() ignored duplicate call in frame={Time.frameCount}", this);
            return;
        }

        lastMenuInvokeFrame = Time.frameCount;
        Debug.Log($"[WorldUIManager] Menu() invoked. beforeToggle visible={isWorldMenuVisible} objectList={isObjectListVisible} status={isStatusVisible} frame={Time.frameCount}", this);
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

        Debug.Log($"[WorldUIManager] Menu() completed. afterToggle visible={isWorldMenuVisible} objectList={isObjectListVisible} status={isStatusVisible} frame={Time.frameCount}", this);
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
        menuRootButton = ResolveButton(menuRootButton, "manu root", "Menu");

        Debug.Log($"[WorldUIManager] EnsureSceneButtonBindings menuRootButton={(menuRootButton != null ? menuRootButton.name : "null")} frame={Time.frameCount}", this);

        EnsureMenuButtonRelay(menuRootButton);
        BindSceneButton(GetObjectListButton(), Onclickbutton1);
        BindSceneButton(GetGenerationButton(), Onclickbutton2);
        BindSceneButton(GetStateButton(), Onclick_State);
        BindSceneButton(GetPageDownButton(), Onclick_PageDown);
        BindSceneButton(GetPageUpButton(), Onclick_PageUp);
        BindSceneButton(GetAdvanceGenerationButton(), Onclickbutton2_1);
        BindSceneButton(GetGenomeViewerButton(), Onclickbutton2_2);
        BindSceneButton(GetGenomeInjectorButton(), Onclickbutton2_3);
    }

    Button GetObjectListButton() => ResolveButton(GetButton(TabUIlist, 0), "list tab");

    Button GetGenerationButton() => ResolveButton(GetButton(TabUIlist, 1), "gen tab");

    Button GetStateButton() => ResolveButton(GetButton(StatusUIlist, 0), "detail");

    Button GetPageDownButton() => ResolveButton(GetButton(StatusUIlist, 1), "<");

    Button GetPageUpButton() => ResolveButton(GetButton(StatusUIlist, 2), ">");

    Button GetAdvanceGenerationButton() => ResolveButton(GetButton(GencontrollerUIlist, 2), "advance gen page", "AdvanceGeneration");

    Button GetGenomeViewerButton() => ResolveButton(GetButton(GencontrollerUIlist, 0), "log page", "Log view");

    Button GetGenomeInjectorButton() => ResolveButton(GetButton(GencontrollerUIlist, 1), "set genome page");

    Button GetButton(List<GameObject> list, int index)
    {
        GameObject go = GetNodeObject(list, index);
        return go != null ? go.GetComponent<Button>() : null;
    }

    Button ResolveButton(Button button, string objectName, string labelText = null)
    {
        if (button != null)
            return button;

        Transform root = mainCanvas != null ? mainCanvas.transform : transform;
        if (root == null)
            return null;

        Transform found = root.Find(objectName);
        if (found != null && found.TryGetComponent<Button>(out var namedButton))
            return namedButton;

        if (string.IsNullOrEmpty(labelText))
            return null;

        foreach (var candidate in root.GetComponentsInChildren<Button>(true))
        {
            TextMeshProUGUI label = candidate.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null && string.Equals(label.text, labelText, StringComparison.Ordinal))
                return candidate;
        }

        return null;
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
        {
            Debug.Log($"[WorldUIManager] {actionName} ignored duplicate call in frame={Time.frameCount}", this);
            return true;
        }

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

        Debug.Log($"[WorldMenuButtonRelay] OnPointerClick button={eventData.button} frame={Time.frameCount}", this);
        manager?.Menu();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Debug.Log($"[WorldMenuButtonRelay] OnSubmit frame={Time.frameCount}", this);
        manager?.Menu();
    }
}
