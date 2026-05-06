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
    public MagicCircleLaunchSettings spellCircleSettings = MagicCircleLaunchSettings.Default;

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
        MagicProjectileLaunchSettings settings = GetProjectileSettings(element);
        Vector3 direction = transform.forward.sqrMagnitude > 0.001f ? transform.forward.normalized : Vector3.down;

        MagicLaunchApi.LaunchWithCharge(this, new MagicLaunchRequest
        {
            element = element,
            origin = transform.position,
            direction = direction,
            spawnOffset = GetProjectileSpawnDistance(direction),
            projectileScale = projectileScale,
            projectileSettings = settings,
            spellCircleSettings = spellCircleSettings,
            projectileName = $"{element} Target Projectile {targetIndex:00}",
            casterResource = GetComponentInParent<Resource>()
        });
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

    MagicProjectileLaunchSettings GetProjectileSettings(MagicElement element)
    {
        return MagicProjectileLaunchSettings.ForElement(element, projectileLifetime, effectLifetime);
    }
}
