using UnityEngine;

public static class SpaceWarpImpactEffect
{
    const string DefaultShaderName = "MagicalEcosystem/Experiment/SpaceWarp";

    public static GameObject CreateWarp(Vector3 point, Vector3 normal, float radius, float lifetime)
    {
        return CreateWarp(point, normal, radius, lifetime, DefaultShaderName);
    }

    public static GameObject CreateWarp(Vector3 point, Vector3 normal, float radius, float lifetime, string shaderName)
    {
        GameObject warp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        warp.name = "Space Warp Impact";

        var collider = warp.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        var renderer = warp.GetComponent<MeshRenderer>();
        renderer.material = CreateWarpMaterial(radius, shaderName);

        var visibility = warp.AddComponent<IceShaderVisibilityController>();
        visibility.maxRenderDistance = 100f;
        visibility.checkInterval = 0.2f;
        visibility.disableOutsideFrustum = true;

        Vector3 impactNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        warp.transform.position = point + impactNormal * (radius * 0.08f);
        warp.transform.up = impactNormal;

        var controller = warp.AddComponent<SpaceWarpImpactController>();
        float effectLifetime = SpaceWarpImpactController.ResolveLifetime(Mathf.Max(0.1f, lifetime));
        controller.Initialize(renderer.material, Mathf.Max(0.1f, radius), effectLifetime);

        Object.Destroy(warp, effectLifetime);
        return warp;
    }

    static Material CreateWarpMaterial(float radius, string shaderName)
    {
        Shader shader = Shader.Find(string.IsNullOrWhiteSpace(shaderName) ? DefaultShaderName : shaderName);
        if (shader == null)
        {
            Debug.LogWarning($"{shaderName} shader was not found. Falling back to Lit, so background distortion will not be visible.");
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }
        if (shader == null)
        {
            Debug.LogWarning("URP Lit shader was not found. Falling back to Standard.");
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_WarpPower"))
            material.SetFloat("_WarpPower", 3f);
        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", 1f);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", 1f);
        if (material.HasProperty("_ObjectRadius"))
            material.SetFloat("_ObjectRadius", Mathf.Max(0.1f, radius));
        if (material.HasProperty("_GlitchEnabled"))
            material.SetFloat("_GlitchEnabled", 0f);
        if (material.HasProperty("_GlitchLineThickness"))
            material.SetFloat("_GlitchLineThickness", 1f);
        if (material.HasProperty("_FresnelColor"))
            material.SetColor("_FresnelColor", new Color(0.45f, 0.85f, 1f, 1f));
        if (material.HasProperty("_FresnelPower"))
            material.SetFloat("_FresnelPower", 3f);
        if (material.HasProperty("_FresnelStrength"))
            material.SetFloat("_FresnelStrength", 0.65f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

}

sealed class SpaceWarpImpactController : MonoBehaviour
{
    const float GrowDuration = 0.5f;
    const float MaxRadiusHoldDuration = 8f;
    const float ShrinkDuration = 0.8f;

    Material material;
    float lifetime;
    float elapsed;
    float startDistortion;
    float startBaseAlpha;
    Vector3 startScale;
    int lastGlitchFrame = -1;
    float currentSeed;

    public static float ResolveLifetime(float requestedLifetime)
    {
        return GrowDuration + MaxRadiusHoldDuration + ShrinkDuration;
    }

    public void Initialize(Material targetMaterial, float radius, float effectLifetime)
    {
        material = targetMaterial;
        lifetime = effectLifetime;
        startScale = Vector3.one * (radius * 2f);
        transform.localScale = startScale;
        UpdateObjectRadius();

        if (material == null)
            return;

        if (material.HasProperty("_DistortionStrength"))
            startDistortion = material.GetFloat("_DistortionStrength");
        if (material.HasProperty("_BaseAlpha"))
            startBaseAlpha = material.GetFloat("_BaseAlpha");
        PickGlitchPixel();
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float shrinkStart = Mathf.Max(GrowDuration + MaxRadiusHoldDuration, lifetime - ShrinkDuration);
        float grow = Mathf.SmoothStep(0.35f, 1f, Mathf.Clamp01(elapsed / GrowDuration));
        float shrink = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((elapsed - shrinkStart) / ShrinkDuration));
        transform.localScale = startScale * grow * shrink;
        UpdateObjectRadius();

        if (material == null)
            return;

        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", startDistortion);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", startBaseAlpha);

        if (Time.frameCount != lastGlitchFrame && Time.frameCount % 3 == 0)
            PickGlitchPixel();
    }

    void UpdateObjectRadius()
    {
        if (material == null || !material.HasProperty("_ObjectRadius"))
            return;

        Vector3 scale = transform.lossyScale;
        float diameter = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        material.SetFloat("_ObjectRadius", Mathf.Max(0.001f, diameter * 0.5f));
    }

    void PickGlitchPixel()
    {
        lastGlitchFrame = Time.frameCount;

        if (material == null)
            return;

        Camera camera = Camera.main;
        if (camera == null || Screen.width <= 0 || Screen.height <= 0)
        {
            SetGlitchEnabled(false);
            return;
        }

        Vector3 center = transform.position;
        float radius = Mathf.Max(0.001f, Mathf.Max(transform.lossyScale.x, Mathf.Max(transform.lossyScale.y, transform.lossyScale.z)) * 0.5f);
        Vector3 cameraRight = camera.transform.right;
        Vector3 cameraUp = camera.transform.up;

        Vector3 centerViewport3 = camera.WorldToViewportPoint(center);
        if (centerViewport3.z <= camera.nearClipPlane)
        {
            SetGlitchEnabled(false);
            return;
        }

        Vector2 centerUV = new Vector2(centerViewport3.x, centerViewport3.y);
        Vector3 rightViewport3 = camera.WorldToViewportPoint(center + cameraRight * radius);
        Vector3 upViewport3 = camera.WorldToViewportPoint(center + cameraUp * radius);
        Vector2 rightRadiusUV = new Vector2(rightViewport3.x, rightViewport3.y) - centerUV;
        Vector2 upRadiusUV = new Vector2(upViewport3.x, upViewport3.y) - centerUV;

        Vector2 plane = Random.insideUnitCircle;
        Vector2 uv = centerUV + rightRadiusUV * plane.x + upRadiusUV * plane.y;
        if (uv.x < 0f || uv.x > 1f || uv.y < 0f || uv.y > 1f)
        {
            SetGlitchEnabled(false);
            return;
        }

        float pixelX = Mathf.Clamp(Mathf.Floor(uv.x * Screen.width) + 0.5f, 0.5f, Screen.width - 0.5f);
        float pixelY = Mathf.Clamp(Mathf.Floor(uv.y * Screen.height) + 0.5f, 0.5f, Screen.height - 0.5f);
        Vector2 pixelUV = new Vector2(pixelX / Screen.width, pixelY / Screen.height);

        if (material.HasProperty("_GlitchPointUV"))
            material.SetVector("_GlitchPointUV", new Vector4(pixelUV.x, pixelUV.y, 0f, 0f));
        if (material.HasProperty("_GlitchSeed"))
        {
            currentSeed += 1f;
            material.SetFloat("_GlitchSeed", currentSeed);
        }

        SetGlitchEnabled(true);
    }

    void SetGlitchEnabled(bool enabled)
    {
        if (material != null && material.HasProperty("_GlitchEnabled"))
            material.SetFloat("_GlitchEnabled", enabled ? 1f : 0f);
    }
}
