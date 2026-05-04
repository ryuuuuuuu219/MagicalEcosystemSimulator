using System;
using UnityEngine;

[DefaultExecutionOrder(-22)]
public class field2AI : MonoBehaviour
{
    [Serializable]
    public struct FieldDamageSettings
    {
        public float heatInflectionPoint;
        public float heatDamageScale;
        public float manaInflectionPoint;
        public float manaDamageScale;
        public float windInflectionPoint;
        public float windDamageScale;
        public float threatInflectionPoint;
        public float threatDamageScale;
    }

    [Serializable]
    public struct FieldEscapeWeights
    {
        public float heatInflectionPoint;
        public float heatEscapeWeight;
        public float manaInflectionPoint;
        public float manaEscapeWeight;
        public float windInflectionPoint;
        public float windEscapeWeight;
        public float threatInflectionPoint;
        public float threatEscapeWeight;
    }

    public static field2AI Instance { get; private set; }

    [Header("Field Managers")]
    public HeatFieldManager heatFieldManager;
    public ManaFieldManager manaFieldManager;
    public WindFieldManager windFieldManager;
    public threatmap_calc threatMap;

    [Header("Damage")]
    public FieldDamageSettings defaultDamageSettings = new FieldDamageSettings
    {
        heatInflectionPoint = 1.2f,
        heatDamageScale = 1f,
        manaInflectionPoint = 8f,
        manaDamageScale = 0f,
        windInflectionPoint = 2f,
        windDamageScale = 0.25f,
        threatInflectionPoint = 10f,
        threatDamageScale = 0f
    };

    [Header("Movement")]
    public float escapeProbeDistance = 4f;
    [Range(4, 16)] public int escapeDirectionSamples = 8;
    public FieldEscapeWeights defaultEscapeWeights = new FieldEscapeWeights
    {
        heatInflectionPoint = 1f,
        heatEscapeWeight = 1f,
        manaInflectionPoint = 8f,
        manaEscapeWeight = 0.2f,
        windInflectionPoint = 1.5f,
        windEscapeWeight = 0.5f,
        threatInflectionPoint = 5f,
        threatEscapeWeight = 1f
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        ResolveManagers();
    }

    public static field2AI GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        field2AI existing = FindFirstObjectByType<field2AI>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("field2AI");
        return go.AddComponent<field2AI>();
    }

    public void ResolveManagers()
    {
        if (heatFieldManager == null)
            heatFieldManager = FindFirstObjectByType<HeatFieldManager>();
        if (manaFieldManager == null)
            manaFieldManager = FindFirstObjectByType<ManaFieldManager>();
        if (windFieldManager == null)
            windFieldManager = FindFirstObjectByType<WindFieldManager>();
        if (threatMap == null)
            threatMap = FindFirstObjectByType<threatmap_calc>();
    }

    #region Damage

    public float GetFieldDamage(Vector3 worldPosition)
    {
        return GetFieldDamage(worldPosition, defaultDamageSettings);
    }

    public float GetFieldDamage(Vector3 worldPosition, FieldDamageSettings settings)
    {
        ResolveManagers();

        float damage = 0f;
        damage += ComputeExcessDamage(SampleHeatMagnitude(worldPosition), settings.heatInflectionPoint, settings.heatDamageScale);
        damage += ComputeExcessDamage(SampleMana(worldPosition), settings.manaInflectionPoint, settings.manaDamageScale);
        damage += ComputeExcessDamage(SampleWindMagnitude(worldPosition), settings.windInflectionPoint, settings.windDamageScale);
        damage += ComputeExcessDamage(SampleThreat(worldPosition), settings.threatInflectionPoint, settings.threatDamageScale);
        return Mathf.Max(0f, damage);
    }

    static float ComputeExcessDamage(float value, float inflectionPoint, float scale)
    {
        return Mathf.Max(0f, value - Mathf.Max(0f, inflectionPoint)) * Mathf.Max(0f, scale);
    }

    #endregion

    #region Movement

    public Vector3 GetSafeZoneVector(Vector3 worldPosition)
    {
        return GetSafeZoneVector(worldPosition, defaultEscapeWeights);
    }

    public Vector3 GetSafeZoneVector(Vector3 worldPosition, FieldEscapeWeights weights)
    {
        ResolveManagers();

        float currentHazard = ComputeEscapeHazard(worldPosition, weights);
        if (currentHazard <= 0f)
            return Vector3.zero;

        int samples = Mathf.Max(4, escapeDirectionSamples);
        float probeDistance = Mathf.Max(0.2f, escapeProbeDistance);
        Vector3 weightedEscape = Vector3.zero;
        Vector3 bestDirection = Vector3.zero;
        float bestHazard = currentHazard;

        for (int i = 0; i < samples; i++)
        {
            float angle = (Mathf.PI * 2f * i) / samples;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            float hazard = ComputeEscapeHazard(worldPosition + dir * probeDistance, weights);
            float improvement = Mathf.Max(0f, currentHazard - hazard);

            if (improvement > 0f)
                weightedEscape += dir * improvement;

            if (hazard < bestHazard)
            {
                bestHazard = hazard;
                bestDirection = dir;
            }
        }

        if (weightedEscape.sqrMagnitude > 0.0001f)
            return weightedEscape.normalized;

        return bestDirection.sqrMagnitude > 0.0001f ? bestDirection.normalized : Vector3.zero;
    }

    float ComputeEscapeHazard(Vector3 worldPosition, FieldEscapeWeights weights)
    {
        float hazard = 0f;
        hazard += ComputeWeightedExcess(SampleHeatMagnitude(worldPosition), weights.heatInflectionPoint, weights.heatEscapeWeight);
        hazard += ComputeWeightedExcess(SampleMana(worldPosition), weights.manaInflectionPoint, weights.manaEscapeWeight);
        hazard += ComputeWeightedExcess(SampleWindMagnitude(worldPosition), weights.windInflectionPoint, weights.windEscapeWeight);
        hazard += ComputeWeightedExcess(SampleThreat(worldPosition), weights.threatInflectionPoint, weights.threatEscapeWeight);
        return hazard;
    }

    static float ComputeWeightedExcess(float value, float inflectionPoint, float weight)
    {
        return Mathf.Max(0f, value - Mathf.Max(0f, inflectionPoint)) * Mathf.Max(0f, weight);
    }

    #endregion

    float SampleHeatMagnitude(Vector3 worldPosition)
    {
        return heatFieldManager != null ? Mathf.Abs(heatFieldManager.SampleHeat(worldPosition)) : 0f;
    }

    float SampleMana(Vector3 worldPosition)
    {
        return manaFieldManager != null ? manaFieldManager.SampleMana(worldPosition) : 0f;
    }

    float SampleWindMagnitude(Vector3 worldPosition)
    {
        return windFieldManager != null ? windFieldManager.SampleWind(worldPosition).magnitude : 0f;
    }

    float SampleThreat(Vector3 worldPosition)
    {
        return threatMap != null ? threatMap.SampleEvaluatedThreat(worldPosition) : 0f;
    }
}
