using UnityEngine;

public class VirtualGaugeManager : MonoBehaviour
{
    public static VirtualGaugeManager Instance { get; private set; }

    [Header("Visibility")]
    [SerializeField] bool showVirtualGauges = true;
    [SerializeField] bool showHerbivoreGauges = true;
    [SerializeField] bool showPredatorGauges = true;
    [SerializeField] bool showHealthGauge = true;
    [SerializeField] bool showManaGauge = true;
    [SerializeField] bool showManaText = true;

    [Header("Appearance")]
    [SerializeField, Range(0f, 1f)] float gaugeAlpha = 1f;

    public bool ShowVirtualGauges => showVirtualGauges;
    public bool ShowHerbivoreGauges => showHerbivoreGauges;
    public bool ShowPredatorGauges => showPredatorGauges;
    public bool ShowHealthGauge => showHealthGauge;
    public bool ShowManaGauge => showManaGauge;
    public bool ShowManaText => showManaText;
    public float GaugeAlpha => gaugeAlpha;

    void Awake()
    {
        Instance = this;
    }

    public void SetGaugeVisibility(bool visible, bool showHerbivores, bool showPredators)
    {
        showVirtualGauges = visible;
        showHerbivoreGauges = showHerbivores;
        showPredatorGauges = showPredators;
    }

    public void SetGaugeDisplayOptions(bool showHealth, bool showManaGaugeValue, bool showManaTextValue, float alpha)
    {
        showHealthGauge = showHealth;
        showManaGauge = showManaGaugeValue;
        showManaText = showManaTextValue;
        gaugeAlpha = Mathf.Clamp01(alpha);
    }
}
