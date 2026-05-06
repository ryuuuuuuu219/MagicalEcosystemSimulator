using System.Collections;
using UnityEngine;

public static class MagicLaunchApi
{
    public static Coroutine LaunchWithCharge(MonoBehaviour runner, MagicLaunchRequest request)
    {
        if (runner == null)
        {
            LaunchImmediate(request);
            return null;
        }

        return runner.StartCoroutine(LaunchWithChargeRoutine(request));
    }

    public static GameObject LaunchImmediate(MagicLaunchRequest request)
    {
        Vector3 direction = request.Direction;
        Vector3 spawnPosition = request.SpawnPosition;
        MagicProjectileLaunchSettings settings = request.projectileSettings.WithFallbackEffectLifetime(5f);

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = string.IsNullOrWhiteSpace(request.projectileName)
            ? $"{request.element} Projectile"
            : request.projectileName;
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one * Mathf.Max(0.01f, request.projectileScale);

        Renderer renderer = projectile.GetComponent<Renderer>();
        renderer.material = CreateProjectileMaterial(settings.projectileColor, 0.9f);

        MagicProjectile magicProjectile = projectile.AddComponent<MagicProjectile>();
        magicProjectile.element = request.element;
        magicProjectile.lifeTime = settings.projectileLifetime;
        magicProjectile.effectRadius = settings.effectRadius;
        magicProjectile.iceSpikeHeight = settings.iceSpikeHeight;
        magicProjectile.iceSpikeRadius = settings.iceSpikeRadius;
        magicProjectile.wrapNonTerrainTargets = settings.wrapNonTerrainTargets;
        magicProjectile.effectLifetime = settings.effectLifetime;
        magicProjectile.envelopeLifetime = settings.envelopeLifetime > 0f ? settings.envelopeLifetime : settings.effectLifetime;
        magicProjectile.envelopePadding = settings.envelopePadding;
        magicProjectile.impactMaterialColor = settings.projectileColor;
        magicProjectile.launchPoint = spawnPosition;
        magicProjectile.projectileSpeed = settings.projectileSpeed;

        Rigidbody body = projectile.GetComponent<Rigidbody>();
        body.linearVelocity = direction * settings.projectileSpeed;
        magicProjectile.SetIntendedVelocity(body.linearVelocity);
        return projectile;
    }

    static IEnumerator LaunchWithChargeRoutine(MagicLaunchRequest request)
    {
        Vector3 direction = request.Direction;
        Vector3 spawnPosition = request.SpawnPosition;
        MagicCircleLaunchSettings circleSettings = request.spellCircleSettings;

        if (circleSettings.radius <= 0f)
            circleSettings = MagicCircleLaunchSettings.Default;

        CustomMagicCircleShaderManager.CreateLaunchCircle(
            request.element,
            spawnPosition,
            direction,
            request.projectileSettings.projectileColor,
            circleSettings);

        if (circleSettings.enabled && circleSettings.chargeDuration > 0f)
            yield return new WaitForSeconds(circleSettings.chargeDuration);

        LaunchImmediate(request);
    }

    static Material CreateProjectileMaterial(Color color, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", smoothness);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
