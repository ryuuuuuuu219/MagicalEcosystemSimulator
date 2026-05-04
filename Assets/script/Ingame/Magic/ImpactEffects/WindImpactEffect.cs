using UnityEngine;

public static class WindImpactEffect
{
    const int SpawnIntervalFrames = 8;
    const float FieldLayerStrength = 18f;
    public const float LayerLifetime = 1.15f;

    public static GameObject Create(Vector3 point, Vector3 normal, float radius, float lifetime)
    {
        GameObject root = new GameObject("Wind Impact");
        float safeRadius = Mathf.Max(0.2f, radius);
        float safeLifetime = Mathf.Max(0.1f, lifetime);
        Vector3 impactNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        root.transform.position = point + impactNormal * (safeRadius * 0.1f);

        var emitter = root.AddComponent<WindImpactEmitter>();
        emitter.Initialize(safeRadius, safeLifetime, SpawnIntervalFrames);

        Object.Destroy(root, safeLifetime);
        return root;
    }

    public static void CreateLayer(Transform parent, string name, float radius, float lifetime, float rotationOffset)
    {
        AddWindFieldLayerPulse(parent, radius, rotationOffset);

        GameObject wind = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wind.name = name;
        wind.transform.SetParent(parent, false);

        var collider = wind.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        wind.transform.localRotation = Quaternion.Euler(
            Random.Range(0f, 360f),
            Random.Range(0f, 360f),
            Random.Range(0f, 360f));
        wind.transform.localScale = Vector3.one * (radius * 1.2f);

        var renderer = wind.GetComponent<MeshRenderer>();
        renderer.material = CreateWindMaterial(rotationOffset);

        var visibility = wind.AddComponent<IceShaderVisibilityController>();
        visibility.maxRenderDistance = 100f;
        visibility.checkInterval = 0.2f;
        visibility.disableOutsideFrustum = true;

        var controller = wind.AddComponent<WindImpactController>();
        float expandTarget = Random.Range(1.45f, 2.15f);
        controller.Initialize(renderer.material, radius, lifetime, expandTarget);

        Object.Destroy(wind, Mathf.Max(0.05f, lifetime));
    }

    static void AddWindFieldLayerPulse(Transform parent, float radius, float rotationOffset)
    {
        if (parent == null)
            return;

        Vector3 direction = new Vector3(Mathf.Cos(rotationOffset), 0f, Mathf.Sin(rotationOffset));
        float strength = FieldLayerStrength * Mathf.Max(0.2f, radius);
        WindFieldManager.GetOrCreate().AddWind(parent.position, direction, strength, Mathf.Max(1f, radius * 1.6f));
    }

    static Material CreateWindMaterial(float rotationOffset)
    {
        Shader shader = Shader.Find("MagicalEcosystem/Experiment/WindSurface");
        if (shader == null)
        {
            Debug.LogWarning("WindSurface shader was not found. Falling back to URP Unlit.");
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        if (material.HasProperty("_LineColor"))
            material.SetColor("_LineColor", new Color(0.78f, 1f, 0.9f, 1f));
        if (material.HasProperty("_DarkLineColor"))
            material.SetColor("_DarkLineColor", new Color(0.04f, 0.16f, 0.1f, 1f));
        if (material.HasProperty("_LineIntensity"))
            material.SetFloat("_LineIntensity", 1.35f);
        if (material.HasProperty("_LineWidth"))
            material.SetFloat("_LineWidth", 0.024f);
        if (material.HasProperty("_StreamCount"))
            material.SetFloat("_StreamCount", 1f);
        if (material.HasProperty("_Twist"))
            material.SetFloat("_Twist", 2.8f);
        if (material.HasProperty("_Speed"))
            material.SetFloat("_Speed", 1.6f);
        if (material.HasProperty("_SurfaceAlpha"))
            material.SetFloat("_SurfaceAlpha", 0.16f);
        if (material.HasProperty("_Fade"))
            material.SetFloat("_Fade", 1f);
        if (material.HasProperty("_RotationOffset"))
            material.SetFloat("_RotationOffset", rotationOffset);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}

sealed class WindImpactEmitter : MonoBehaviour
{
    float radius;
    float lifetime;
    float elapsed;
    int spawnIntervalFrames;
    int frameCounter;
    int burstIndex;

    public void Initialize(float effectRadius, float effectLifetime, int intervalFrames)
    {
        radius = effectRadius;
        lifetime = effectLifetime;
        spawnIntervalFrames = Mathf.Max(1, intervalFrames);
        SpawnBurst();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= lifetime)
            return;

        frameCounter++;
        if (frameCounter < spawnIntervalFrames)
            return;

        frameCounter = 0;
        SpawnBurst();
    }

    void SpawnBurst()
    {
        float remainingLifetime = Mathf.Max(0.05f, lifetime - elapsed);
        float layerLifetime = Mathf.Min(WindImpactEffect.LayerLifetime, remainingLifetime);
        string suffix = burstIndex.ToString("00");
        WindImpactEffect.CreateLayer(transform, $"Wind Impact Layer A {suffix}", radius, layerLifetime, Random.Range(0f, Mathf.PI * 2f));
        WindImpactEffect.CreateLayer(transform, $"Wind Impact Layer B {suffix}", radius * 1.08f, layerLifetime, Random.Range(0f, Mathf.PI * 2f));
        burstIndex++;
    }
}

sealed class WindImpactController : MonoBehaviour
{
    Material material;
    float lifetime;
    float elapsed;
    float startFade;
    float expandTarget = 1.8f;
    Vector3 startScale;

    public void Initialize(Material targetMaterial, float radius, float effectLifetime, float targetExpand)
    {
        material = targetMaterial;
        lifetime = effectLifetime;
        expandTarget = Mathf.Max(0.1f, targetExpand);
        startScale = Vector3.one * (radius * 1.2f);
        transform.localScale = startScale;

        if (material != null && material.HasProperty("_Fade"))
            startFade = material.GetFloat("_Fade");
        else
            startFade = 1f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
        float expand = Mathf.SmoothStep(0.65f, expandTarget, t);
        float fade = 1f - Mathf.SmoothStep(0.55f, 1f, t);

        transform.localScale = startScale * expand;

        if (material != null && material.HasProperty("_Fade"))
            material.SetFloat("_Fade", startFade * fade);
    }
}
