using UnityEngine;

public class MagicExperimentTarget : MonoBehaviour
{
    public int targetIndex;
    public Vector3 terrainNormal = Vector3.up;
    public Vector3 spawnedScale = Vector3.one;
    public bool terrainAligned;

    [Header("Rotation")]
    public bool randomizeForwardOnStart = true;
    public float minDownwardTilt = 8f;
    public float maxDownwardTilt = 48f;
    public float yRotationSpeed = 35f;
    public float yRotationSpeedJitter = 18f;

    [Header("Auto Fire")]
    public bool autoFire = true;
    public float minFireInterval = 1.2f;
    public float maxFireInterval = 3.2f;
    public float projectileSpawnOffset = 1.1f;
    public float projectileScale = 0.36f;
    public float projectileLifetime = 8f;
    public float effectLifetime = 6f;

    float currentYRotationSpeed;
    float nextFireTime;
    static readonly MagicElement[] FireElements =
    {
        MagicElement.Fire,
        MagicElement.Ice,
        MagicElement.Lightning,
        MagicElement.Wind,
        MagicElement.Space
    };

    public void Initialize(int index, Vector3 normal, Vector3 scale, bool alignedToTerrain)
    {
        targetIndex = index;
        terrainNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        spawnedScale = scale;
        terrainAligned = alignedToTerrain;
    }

    void Start()
    {
        if (randomizeForwardOnStart)
            ApplyRandomDownwardRotation();

        float speedSign = Random.value < 0.5f ? -1f : 1f;
        currentYRotationSpeed = (yRotationSpeed + Random.Range(-yRotationSpeedJitter, yRotationSpeedJitter)) * speedSign;
        ScheduleNextFire();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, currentYRotationSpeed * Time.deltaTime, Space.World);

        if (!autoFire || Time.time < nextFireTime)
            return;

        FireRandomMagic();
        ScheduleNextFire();
    }

    void ApplyRandomDownwardRotation()
    {
        Vector3 down = terrainNormal.sqrMagnitude > 0.001f ? -terrainNormal.normalized : Vector3.down;
        Vector3 tangent = Vector3.ProjectOnPlane(Random.onUnitSphere, down);
        if (tangent.sqrMagnitude < 0.001f)
            tangent = Vector3.ProjectOnPlane(Vector3.forward, down);
        if (tangent.sqrMagnitude < 0.001f)
            tangent = Vector3.right;

        Vector3 axis = Vector3.Cross(down, tangent.normalized);
        float tilt = Random.Range(Mathf.Min(minDownwardTilt, maxDownwardTilt), Mathf.Max(minDownwardTilt, maxDownwardTilt));
        Vector3 forward = Quaternion.AngleAxis(tilt, axis.normalized) * down;
        Quaternion baseRotation = Quaternion.LookRotation(forward.normalized, terrainNormal);
        transform.rotation = baseRotation * Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward);
    }

    void ScheduleNextFire()
    {
        nextFireTime = Time.time + Random.Range(Mathf.Min(minFireInterval, maxFireInterval), Mathf.Max(minFireInterval, maxFireInterval));
    }

    void FireRandomMagic()
    {
        MagicElement element = FireElements[Random.Range(0, FireElements.Length)];
        ProjectileSettings settings = GetProjectileSettings(element);
        Vector3 direction = transform.forward.sqrMagnitude > 0.001f ? transform.forward.normalized : Vector3.down;
        Vector3 spawnPosition = transform.position + direction * GetProjectileSpawnDistance(direction);

        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = $"{element} Target Projectile {targetIndex:00}";
        projectile.transform.position = spawnPosition;
        projectile.transform.localScale = Vector3.one * projectileScale;

        var renderer = projectile.GetComponent<Renderer>();
        renderer.material = CreateMaterial(settings.projectileColor, 0.9f);

        var magicProjectile = projectile.AddComponent<MagicProjectile>();
        magicProjectile.element = element;
        magicProjectile.lifeTime = settings.projectileLifetime;
        magicProjectile.effectRadius = settings.effectRadius;
        magicProjectile.iceSpikeHeight = settings.iceSpikeHeight;
        magicProjectile.iceSpikeRadius = settings.iceSpikeRadius;
        magicProjectile.wrapNonTerrainTargets = settings.wrapNonTerrainTargets;
        magicProjectile.effectLifetime = settings.effectLifetime;
        magicProjectile.envelopeLifetime = settings.effectLifetime;
        magicProjectile.envelopePadding = settings.envelopePadding;
        magicProjectile.impactMaterialColor = settings.projectileColor;
        magicProjectile.launchPoint = spawnPosition;
        magicProjectile.projectileSpeed = settings.projectileSpeed;

        var body = projectile.GetComponent<Rigidbody>();
        body.linearVelocity = direction * settings.projectileSpeed;
    }

    float GetProjectileSpawnDistance(Vector3 direction)
    {
        var col = GetComponent<Collider>();
        if (col == null)
            return projectileSpawnOffset;

        Bounds bounds = col.bounds;
        Vector3 extents = bounds.extents;
        float projectedExtent =
            Mathf.Abs(direction.x) * extents.x +
            Mathf.Abs(direction.y) * extents.y +
            Mathf.Abs(direction.z) * extents.z;
        return projectedExtent + projectileSpawnOffset;
    }

    ProjectileSettings GetProjectileSettings(MagicElement element)
    {
        switch (element)
        {
            case MagicElement.Fire:
                return new ProjectileSettings(new Color(1f, 0.35f, 0.08f, 0.85f), 55f, projectileLifetime, 3f, 0f, 0f, false, effectLifetime, 0.2f);
            case MagicElement.Ice:
                return new ProjectileSettings(new Color(0.55f, 0.9f, 1f, 0.8f), 60f, projectileLifetime, 2f, 3f, 0.6f, true, effectLifetime, 0.25f);
            case MagicElement.Lightning:
                return new ProjectileSettings(new Color(1f, 0.95f, 0.25f, 0.9f), 120f, 4f, 1.5f, 0f, 0f, false, 3f, 0.15f);
            case MagicElement.Wind:
                return new ProjectileSettings(new Color(0.65f, 1f, 0.75f, 0.45f), 75f, 6f, 4f, 0f, 0f, false, 4f, 0.3f);
            case MagicElement.Space:
                return new ProjectileSettings(new Color(0.75f, 0.45f, 1f, 0.7f), 50f, 7f, 2.5f, 0f, 0f, false, 5f, 0.25f);
            default:
                return new ProjectileSettings(new Color(0.55f, 0.9f, 1f, 0.8f), 60f, projectileLifetime, 2f, 3f, 0.6f, true, effectLifetime, 0.25f);
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

    struct ProjectileSettings
    {
        public readonly Color projectileColor;
        public readonly float projectileSpeed;
        public readonly float projectileLifetime;
        public readonly float effectRadius;
        public readonly float iceSpikeHeight;
        public readonly float iceSpikeRadius;
        public readonly bool wrapNonTerrainTargets;
        public readonly float effectLifetime;
        public readonly float envelopePadding;

        public ProjectileSettings(
            Color projectileColor,
            float projectileSpeed,
            float projectileLifetime,
            float effectRadius,
            float iceSpikeHeight,
            float iceSpikeRadius,
            bool wrapNonTerrainTargets,
            float effectLifetime,
            float envelopePadding)
        {
            this.projectileColor = projectileColor;
            this.projectileSpeed = projectileSpeed;
            this.projectileLifetime = projectileLifetime;
            this.effectRadius = effectRadius;
            this.iceSpikeHeight = iceSpikeHeight;
            this.iceSpikeRadius = iceSpikeRadius;
            this.wrapNonTerrainTargets = wrapNonTerrainTargets;
            this.effectLifetime = effectLifetime;
            this.envelopePadding = envelopePadding;
        }
    }
}
