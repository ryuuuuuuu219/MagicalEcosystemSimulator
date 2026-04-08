using UnityEngine;

public class VirtualGaugeManager : MonoBehaviour
{
    public static VirtualGaugeManager Instance { get; private set; }

    [Header("Visibility")]
    [SerializeField] bool showVirtualGauges = true;
    [SerializeField] bool showHerbivoreGauges = true;
    [SerializeField] bool showPredatorGauges = true;
    [SerializeField] bool showHealthGauge = true;
    [SerializeField] bool showEnergyGauge = true;
    [SerializeField] bool showCarbonText = true;

    [Header("Appearance")]
    [SerializeField, Range(0f, 1f)] float gaugeAlpha = 1f;

    public bool ShowVirtualGauges => showVirtualGauges;
    public bool ShowHerbivoreGauges => showHerbivoreGauges;
    public bool ShowPredatorGauges => showPredatorGauges;
    public bool ShowHealthGauge => showHealthGauge;
    public bool ShowEnergyGauge => showEnergyGauge;
    public bool ShowCarbonText => showCarbonText;
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

    public void SetGaugeDisplayOptions(bool showHealth, bool showEnergy, bool showCarbon, float alpha)
    {
        showHealthGauge = showHealth;
        showEnergyGauge = showEnergy;
        showCarbonText = showCarbon;
        gaugeAlpha = Mathf.Clamp01(alpha);
    }
}
