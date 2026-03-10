using System.Collections.Generic;
using TMPro;
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
    const int stateViewPageCount = 5;
    string currentHerbivoreDnaCode = string.Empty;
    Button herbivoreDnaCopyButton;
    bool IsStateViewVisible => isStatusVisible && isObjectListVisible;

    void Update()
    {
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

        if (!isWorldMenuVisible && Input.GetMouseButtonDown(0))
        {
            Objectpic();

            if (currentTarget == null) return;

            isWorldMenuVisible = true;
            isObjectListVisible = true;
            foreach (var go in TabUIlist)
            {
                go.SetActive(true);
            }

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

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, selectableLayer))
        {
            SetTarget(hit.collider.gameObject);
            Menu();
        }
    }

    public void Menu()
    {
        isWorldMenuVisible = !isWorldMenuVisible;
        foreach (GameObject go in TabUIlist)
        {
            go.SetActive(isWorldMenuVisible);
        }
        if (!isWorldMenuVisible)
        {
            isObjectListVisible = false;
            isStatusVisible = false;

            currentTarget = null;
            ClearObjectList();
            HideStatusUI();
            ClearStateview();
            UpdateFollowText();
        }
    }
    public void Onclickbutton1()
    {
        BuildObjectList();

        foreach (GameObject go in GencontrollerUIlist)
        {
            go.SetActive(false);
        }

    }
    public void Onclickbutton2()
    {
        foreach (GameObject go in GencontrollerUIlist)
        {
            go.SetActive(true);
        }
        ClearObjectList();
        HideStatusUI();
        ClearStateview();

    }

    public void Onclickbutton2_1()
    {
        //advance gen
        if (generationController != null)
            generationController.onclickbutton2_1();
    }

    public void Onclickbutton2_2()
    {
        //genome view
        if (generationController != null)
            generationController.onclickbutton2_2();
    }

    public void Onclickbutton2_3()
    {
        //set genome
        if (generationController != null)
            generationController.onclickbutton2_3();
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
            statusText += "\ndead:" + herbivore.IsDead;
        }
        else if (currentTarget.TryGetComponent<predatorBehaviour>(out var predator))
        {
            statusText += "\nhealth:" + predator.health.ToString("F1");
            statusText += "\ndead:" + predator.IsDead;
        }

        text_f.text = statusText;
    }
}
