using UnityEngine;

public class MagicMaterialExperimentLauncher : MonoBehaviour
{
    [System.Serializable]
    public struct ProjectileLaunchSettings
    {
        public Color projectileColor;
        public float projectileSpeed;
        public float projectileLifetime;
        public float effectRadius;
        public float iceSpikeHeight;
        public float iceSpikeRadius;
        public bool wrapNonTerrainTargets;
        public float envelopeLifetime;
        public float envelopePadding;
    }

    public Camera sourceCamera;
    public MagicElement launchElement = MagicElement.Ice;
    public float lifetime = 8f;
    public float effectLifetime = 6f;
    public float projectileSpawnOffset = 1.2f;
    public ProjectileLaunchSettings currentLaunchSettings;

    [Header("Launch Settings")]
    public ProjectileLaunchSettings fireLaunchSettings = new ProjectileLaunchSettings
    {
        projectileColor = new Color(1f, 0.35f, 0.08f, 0.85f),
        projectileSpeed = 55f,
        projectileLifetime = 8f,
        effectRadius = 3f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        envelopeLifetime = 5f,
        envelopePadding = 0.2f
    };
    public ProjectileLaunchSettings iceLaunchSettings = new ProjectileLaunchSettings
    {
        projectileColor = new Color(0.55f, 0.9f, 1f, 0.8f),
        projectileSpeed = 60f,
        projectileLifetime = 8f,
        effectRadius = 2f,
        iceSpikeHeight = 3f,
        iceSpikeRadius = 0.6f,
        wrapNonTerrainTargets = true,
        envelopeLifetime = 6f,
        envelopePadding = 0.25f
    };
    public ProjectileLaunchSettings lightningLaunchSettings = new ProjectileLaunchSettings
    {
        projectileColor = new Color(1f, 0.95f, 0.25f, 0.9f),
        projectileSpeed = 120f,
        projectileLifetime = 4f,
        effectRadius = 1.5f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        envelopeLifetime = 3f,
        envelopePadding = 0.15f
    };
    public ProjectileLaunchSettings windLaunchSettings = new ProjectileLaunchSettings
    {
        projectileColor = new Color(0.65f, 1f, 0.75f, 0.45f),
        projectileSpeed = 75f,
        projectileLifetime = 6f,
        effectRadius = 4f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        envelopeLifetime = 4f,
        envelopePadding = 0.3f
    };
    public ProjectileLaunchSettings spaceLaunchSettings = new ProjectileLaunchSettings
    {
        projectileColor = new Color(0.75f, 0.45f, 1f, 0.7f),
        projectileSpeed = 50f,
        projectileLifetime = 7f,
        effectRadius = 2.5f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        envelopeLifetime = 5f,
        envelopePadding = 0.25f
    };
    void Start()
    {
        if (sourceCamera == null)
            sourceCamera = Camera.main;
    }

    void Update()
    {
        if (sourceCamera == null)
            return;

        HandleElementHotkeys();

        if (Input.GetMouseButtonDown(0))
        {
            AssignLaunchSettingsByElement();
            LaunchProjectileFromCamera();
        }
    }

    void HandleElementHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            launchElement = MagicElement.Fire;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            launchElement = MagicElement.Ice;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            launchElement = MagicElement.Lightning;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            launchElement = MagicElement.Wind;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            launchElement = MagicElement.Space;
    }

    void LaunchProjectileFromCamera()
    {
        Ray ray = sourceCamera.ScreenPointToRay(Input.mousePosition);
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = $"{launchElement} Launched Projectile";
        projectile.transform.position = ray.origin + ray.direction * projectileSpawnOffset;
        projectile.transform.localScale = Vector3.one * 0.36f;

        var renderer = projectile.GetComponent<Renderer>();
        renderer.material = CreateMaterial(currentLaunchSettings.projectileColor, 0.9f);

        var magicProjectile = projectile.AddComponent<MagicProjectile>();
        magicProjectile.element = launchElement;
        magicProjectile.lifeTime = lifetime;
        magicProjectile.effectRadius = currentLaunchSettings.effectRadius;
        magicProjectile.iceSpikeHeight = currentLaunchSettings.iceSpikeHeight;
        magicProjectile.iceSpikeRadius = currentLaunchSettings.iceSpikeRadius;
        magicProjectile.wrapNonTerrainTargets = currentLaunchSettings.wrapNonTerrainTargets;
        magicProjectile.effectLifetime = effectLifetime;
        magicProjectile.envelopeLifetime = effectLifetime;
        magicProjectile.envelopePadding = currentLaunchSettings.envelopePadding;
        magicProjectile.impactMaterialColor = currentLaunchSettings.projectileColor;
        magicProjectile.launchPoint = projectile.transform.position;
        magicProjectile.projectileSpeed = currentLaunchSettings.projectileSpeed;

        var body = projectile.GetComponent<Rigidbody>();
        body.linearVelocity = ray.direction.normalized * currentLaunchSettings.projectileSpeed;
    }

    void AssignLaunchSettingsByElement()
    {
        switch (launchElement)
        {
            case MagicElement.Fire:
                currentLaunchSettings = fireLaunchSettings;
                break;
            case MagicElement.Ice:
                currentLaunchSettings = iceLaunchSettings;
                break;
            case MagicElement.Lightning:
                currentLaunchSettings = lightningLaunchSettings;
                break;
            case MagicElement.Wind:
                currentLaunchSettings = windLaunchSettings;
                break;
            case MagicElement.Space:
                currentLaunchSettings = spaceLaunchSettings;
                break;
            default:
                currentLaunchSettings = iceLaunchSettings;
                break;
        }
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
