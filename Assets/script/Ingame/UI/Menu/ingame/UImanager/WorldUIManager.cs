using TMPro;
using UnityEngine;

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

    void Start()
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
        HideInitialUiObjects();
    }

    void Update()
    {
        UpdateVirtualGauges();
        UpdateCameraFollow();
        HandleWorldClick();
    }

    void HideInitialUiObjects()
    {
        foreach (GameObject go in EnumerateInitialUiObjects())
        {
            if (go != null)
                go.SetActive(false);
        }
    }
}
