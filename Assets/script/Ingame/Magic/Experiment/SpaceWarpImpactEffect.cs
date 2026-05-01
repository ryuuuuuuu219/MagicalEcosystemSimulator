using UnityEngine;

public static class SpaceWarpImpactEffect
{
    public static GameObject CreateWarp(Vector3 point, Vector3 normal, float radius, float lifetime)
    {
        GameObject warp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        warp.name = "Space Warp Impact";

        var collider = warp.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        var renderer = warp.GetComponent<MeshRenderer>();
        renderer.material = CreateWarpMaterial(radius);

        var visibility = warp.AddComponent<IceShaderVisibilityController>();
        visibility.maxRenderDistance = 100f;
        visibility.checkInterval = 0.2f;
        visibility.disableOutsideFrustum = true;

        Vector3 impactNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        warp.transform.position = point + impactNormal * (radius * 0.08f);
        warp.transform.up = impactNormal;

        var controller = warp.AddComponent<SpaceWarpImpactController>();
        controller.Initialize(renderer.material, Mathf.Max(0.1f, radius), Mathf.Max(0.1f, lifetime));

        Object.Destroy(warp, Mathf.Max(0.1f, lifetime));
        return warp;
    }

    static Material CreateWarpMaterial(float radius)
    {
        Shader shader = Shader.Find("MagicalEcosystem/Experiment/SpaceWarp");
        if (shader == null)
        {
            Debug.LogWarning("SpaceWarp shader was not found. Falling back to Lit, so background distortion will not be visible.");
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }
        if (shader == null)
        {
            Debug.LogWarning("URP Lit shader was not found. Falling back to Standard.");
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_WarpPower"))
            material.SetFloat("_WarpPower", 2f);
        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", 0.24f);
        if (material.HasProperty("_RadialPushPull"))
            material.SetFloat("_RadialPushPull", -0.42f);
        if (material.HasProperty("_AnchorSharpness"))
            material.SetFloat("_AnchorSharpness", 10f);
        if (material.HasProperty("_AnchorRandomness"))
            material.SetFloat("_AnchorRandomness", 0.72f);
        if (material.HasProperty("_CenterBiasStrength"))
            material.SetFloat("_CenterBiasStrength", 0.38f);
        if (material.HasProperty("_BoundaryConnectPower"))
            material.SetFloat("_BoundaryConnectPower", 1.8f);
        if (material.HasProperty("_EdgeStart"))
            material.SetFloat("_EdgeStart", 0.7f);
        if (material.HasProperty("_DarkenStrength"))
            material.SetFloat("_DarkenStrength", 2.1f);
        if (material.HasProperty("_CenterDarkness"))
            material.SetFloat("_CenterDarkness", 0.86f);
        if (material.HasProperty("_CenterRadius"))
            material.SetFloat("_CenterRadius", 0.32f);
        if (material.HasProperty("_FresnelPower"))
            material.SetFloat("_FresnelPower", 2.7f);
        if (material.HasProperty("_RimColor"))
            material.SetColor("_RimColor", new Color(0.68f, 0.46f, 1f, 1f));
        if (material.HasProperty("_RimIntensity"))
            material.SetFloat("_RimIntensity", 1.15f);
        if (material.HasProperty("_RimAlphaBoost"))
            material.SetFloat("_RimAlphaBoost", 0.48f);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", 0.62f);
        if (material.HasProperty("_ScreenRadius"))
            material.SetFloat("_ScreenRadius", Mathf.Clamp(radius * 0.065f, 0.08f, 0.28f));

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

}

sealed class SpaceWarpImpactController : MonoBehaviour
{
    Material material;
    float lifetime;
    float elapsed;
    float startDistortion;
    float startBaseAlpha;
    float startRimIntensity;
    float startScreenRadius;
    Vector3 startScale;

    public void Initialize(Material targetMaterial, float radius, float effectLifetime)
    {
        material = targetMaterial;
        lifetime = effectLifetime;
        startScale = Vector3.one * (radius * 2f);
        transform.localScale = startScale;

        if (material == null)
            return;

        if (material.HasProperty("_DistortionStrength"))
            startDistortion = material.GetFloat("_DistortionStrength");
        if (material.HasProperty("_BaseAlpha"))
            startBaseAlpha = material.GetFloat("_BaseAlpha");
        if (material.HasProperty("_RimIntensity"))
            startRimIntensity = material.GetFloat("_RimIntensity");
        if (material.HasProperty("_ScreenRadius"))
            startScreenRadius = material.GetFloat("_ScreenRadius");
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
        float grow = Mathf.SmoothStep(0.35f, 1.05f, Mathf.Clamp01(t * 2.2f));
        float fade = 1f - Mathf.SmoothStep(0.55f, 1f, t);
        transform.localScale = startScale * grow;

        if (material == null)
            return;

        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", startDistortion * fade);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", startBaseAlpha * fade);
        if (material.HasProperty("_RimIntensity"))
            material.SetFloat("_RimIntensity", startRimIntensity * fade);
        if (material.HasProperty("_ScreenRadius"))
            material.SetFloat("_ScreenRadius", Mathf.Lerp(startScreenRadius * 1.25f, startScreenRadius * 0.65f, t));
    }
}
