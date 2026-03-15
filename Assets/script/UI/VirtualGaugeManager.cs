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

    public bool ShowVirtualGauges => showVirtualGauges;
    public bool ShowHerbivoreGauges => showHerbivoreGauges;
    public bool ShowPredatorGauges => showPredatorGauges;
    public bool ShowHealthGauge => showHealthGauge;
    public bool ShowEnergyGauge => showEnergyGauge;
    public bool ShowCarbonText => showCarbonText;

    void Awake()
    {
        Instance = this;
    }
}
