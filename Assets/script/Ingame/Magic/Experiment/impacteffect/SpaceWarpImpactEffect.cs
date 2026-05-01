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
            material.SetFloat("_WarpPower", 3f);
        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", 1f);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", 1f);
        if (material.HasProperty("_ObjectRadius"))
            material.SetFloat("_ObjectRadius", Mathf.Max(0.1f, radius));
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
    Vector3 startScale;

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
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
        float grow = Mathf.SmoothStep(0.35f, 1.05f, Mathf.Clamp01(t * 2.2f));
        float fade = 1f - Mathf.SmoothStep(0.55f, 1f, t);
        transform.localScale = startScale * grow;
        UpdateObjectRadius();

        if (material == null)
            return;

        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", startDistortion);
        if (material.HasProperty("_BaseAlpha"))
            material.SetFloat("_BaseAlpha", startBaseAlpha);
    }

    void UpdateObjectRadius()
    {
        if (material == null || !material.HasProperty("_ObjectRadius"))
            return;

        Vector3 scale = transform.lossyScale;
        float diameter = Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        material.SetFloat("_ObjectRadius", Mathf.Max(0.001f, diameter * 0.5f));
    }
}
