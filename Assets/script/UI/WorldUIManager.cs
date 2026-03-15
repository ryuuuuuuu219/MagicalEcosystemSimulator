using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
    bool IsStateViewVisible => isStatusVisible && isObjectListVisible;

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
        if (EventSystem.current.IsPointerOverGameObject())
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

    public void Menu()
    {
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
    public void Onclickbutton1()
    {
        OpenObjectListBranch();
    }
    public void Onclickbutton2()
    {
        OpenGenerationBranch();
    }

    public void Onclickbutton2_1()
    {
        OpenAdvanceGenerationBranch();
    }

    public void Onclickbutton2_2()
    {
        OpenGenomeViewerBranch();
    }

    public void Onclickbutton2_3()
    {
        OpenGenomeInjectorBranch();
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
}
