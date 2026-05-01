using UnityEngine;

public class MagicMaterialExperimentLauncher : MonoBehaviour
{
    public Camera sourceCamera;
    public MagicAttributeManager attributeManager;
    public MagicElement launchElement = MagicElement.Ice;
    public float fallbackLaunchSpeed = 60f;
    public float fallbackProjectileLifetime = 8f;
    public float projectileSpawnOffset = 1.2f;
    public float iceTargetDistance = 18f;
    public Vector3 iceTargetSize = new Vector3(4f, 4f, 1f);

    void Start()
    {
        if (sourceCamera == null)
            sourceCamera = Camera.main;
        if (attributeManager == null)
            attributeManager = FindFirstObjectByType<MagicAttributeManager>();

        EnsureIceTestTarget();
    }

    void Update()
    {
        if (sourceCamera == null)
            return;

        if (Input.GetMouseButtonDown(0))
            LaunchProjectileFromCamera();
    }

    void LaunchProjectileFromCamera()
    {
        MagicAttributeManager.AttributeDefinition definition = GetLaunchDefinition();
        Ray ray = sourceCamera.ScreenPointToRay(Input.mousePosition);
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = $"{launchElement} Launched Projectile";
        projectile.transform.position = ray.origin + ray.direction * projectileSpawnOffset;
        projectile.transform.localScale = Vector3.one * 0.36f;

        var renderer = projectile.GetComponent<Renderer>();
        renderer.material = CreateMaterial(definition.projectileColor, 0.9f);

        var magicProjectile = projectile.AddComponent<MagicProjectile>();
        magicProjectile.element = launchElement;
        magicProjectile.lifeTime = definition.projectileLifetime;
        magicProjectile.effectRadius = definition.effectRadius;
        magicProjectile.iceSpikeHeight = definition.iceSpikeHeight;
        magicProjectile.iceSpikeRadius = definition.iceSpikeRadius;
        magicProjectile.wrapNonTerrainTargets = definition.wrapNonTerrainTargets;
        magicProjectile.envelopeLifetime = definition.envelopeLifetime;
        magicProjectile.envelopePadding = definition.envelopePadding;
        magicProjectile.impactMaterialColor = definition.projectileColor;

        var body = projectile.GetComponent<Rigidbody>();
        body.linearVelocity = ray.direction.normalized * definition.projectileSpeed;
    }

    void EnsureIceTestTarget()
    {
        if (GameObject.Find("Ice Experiment Target") != null || sourceCamera == null)
            return;

        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
        target.name = "Ice Experiment Target";
        target.transform.position = sourceCamera.transform.position + sourceCamera.transform.forward * iceTargetDistance;
        target.transform.localScale = iceTargetSize;
        target.transform.rotation = Quaternion.LookRotation(sourceCamera.transform.forward, Vector3.up);

        var renderer = target.GetComponent<Renderer>();
        renderer.material = CreateIceTargetMaterial();
    }

    MagicAttributeManager.AttributeDefinition GetLaunchDefinition()
    {
        if (attributeManager != null)
            return attributeManager.GetDefinition(launchElement);

        return new MagicAttributeManager.AttributeDefinition
        {
            element = launchElement,
            projectileColor = new Color(0.55f, 0.9f, 1f, 0.8f),
            projectileSpeed = fallbackLaunchSpeed,
            projectileLifetime = fallbackProjectileLifetime,
            effectRadius = 2f,
            iceSpikeHeight = 3f,
            iceSpikeRadius = 0.6f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 5f,
            envelopePadding = 0.2f
        };
    }

    Material CreateIceTargetMaterial()
    {
        return CreateMaterial(new Color(0.35f, 0.75f, 1f, 0.35f), 0.65f);
    }

    static Material CreateMaterial(Color color, float smoothness)
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
