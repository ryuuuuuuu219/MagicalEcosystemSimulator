using UnityEngine;

public static class MagicFieldImpactEffect
{
    const float FireHeatAmount = 180f;
    const float IceHeatAmount = -130f;
    const float SpaceManaAmount = 160f;
    const float WindMainStrength = 1.8f;
    const float WindPulseStrength = 0.45f;

    public static void Apply(MagicElement element, Vector3 point, Vector3 normal, float radius, float lifetime)
    {
        float safeRadius = Mathf.Max(0.2f, radius);
        float safeLifetime = Mathf.Max(0.1f, lifetime);
        threatmap_calc threatMap = Object.FindFirstObjectByType<threatmap_calc>();
        if (threatMap != null)
            threatMap.AddThreatPulse(new Vector2(point.x, point.z), safeRadius * 3f, 80f);

        switch (element)
        {
            case MagicElement.Fire:
                HeatFieldManager.GetOrCreate().AddHeat(point, FireHeatAmount, safeRadius * 1.6f);
                break;
            case MagicElement.Ice:
                HeatFieldManager.GetOrCreate().AddHeat(point, IceHeatAmount, safeRadius * 1.4f);
                break;
            case MagicElement.Space:
                ManaFieldManager.GetOrCreate().AddMana(point, SpaceManaAmount, safeRadius * 1.8f);
                break;
            case MagicElement.Wind:
                CreateWindFieldProxy(point, normal, safeRadius, safeLifetime);
                break;
            case MagicElement.Lightning:
                // 設計メモ上、雷は常設 field ではなく短時間 event として扱う。
                break;
        }
    }

    static void CreateWindFieldProxy(Vector3 point, Vector3 normal, float radius, float lifetime)
    {
        Vector3 direction = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        GameObject root = new GameObject("Wind Field Proxy");
        root.transform.position = point + direction * Mathf.Max(0.5f, radius * 0.4f);
        root.transform.rotation = Quaternion.LookRotation(direction);

        WindZone windZone = root.AddComponent<WindZone>();
        windZone.mode = WindZoneMode.Spherical;
        windZone.radius = Mathf.Max(4f, radius * 4f);
        windZone.windMain = WindMainStrength;
        windZone.windPulseMagnitude = WindPulseStrength;
        windZone.windPulseFrequency = 0.7f;
        windZone.windTurbulence = 0.65f;

        WindFieldProxyController controller = root.AddComponent<WindFieldProxyController>();
        controller.Initialize(windZone, lifetime);

        Object.Destroy(root, lifetime);
    }
}

sealed class WindFieldProxyController : MonoBehaviour
{
    WindZone windZone;
    float lifetime;
    float elapsed;
    float startMain;
    float startPulse;
    float startTurbulence;

    public void Initialize(WindZone targetWindZone, float effectLifetime)
    {
        windZone = targetWindZone;
        lifetime = Mathf.Max(0.1f, effectLifetime);

        if (windZone == null)
            return;

        startMain = windZone.windMain;
        startPulse = windZone.windPulseMagnitude;
        startTurbulence = windZone.windTurbulence;
    }

    void Update()
    {
        if (windZone == null)
            return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);
        float fade = 1f - Mathf.SmoothStep(0.35f, 1f, t);
        windZone.windMain = startMain * fade;
        windZone.windPulseMagnitude = startPulse * fade;
        windZone.windTurbulence = startTurbulence * fade;
    }
}
