using UnityEngine;
using UnityEngine.EventSystems;

public class MagicMaterialExperimentLauncher : MonoBehaviour
{
    public Camera sourceCamera;
    public MagicElement launchElement = MagicElement.Ice;
    public float lifetime = 8f;
    public float effectLifetime = 6f;
    public float projectileSpawnOffset = 1.2f;
    public float projectileScale = 0.36f;
    public MagicCircleLaunchSettings spellCircleSettings = MagicCircleLaunchSettings.Default;
    public MagicProjectileLaunchSettings currentLaunchSettings;
    bool isCharging;

    [Header("Launch Settings")]
    public MagicProjectileLaunchSettings fireLaunchSettings = new MagicProjectileLaunchSettings
    {
        projectileColor = new Color(1f, 0.35f, 0.08f, 0.85f),
        projectileSpeed = 55f,
        projectileLifetime = 8f,
        effectRadius = 3f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        effectLifetime = 5f,
        envelopePadding = 0.2f
    };
    public MagicProjectileLaunchSettings iceLaunchSettings = new MagicProjectileLaunchSettings
    {
        projectileColor = new Color(0.55f, 0.9f, 1f, 0.8f),
        projectileSpeed = 60f,
        projectileLifetime = 8f,
        effectRadius = 2f,
        iceSpikeHeight = 3f,
        iceSpikeRadius = 0.6f,
        wrapNonTerrainTargets = true,
        effectLifetime = 6f,
        envelopePadding = 0.25f
    };
    public MagicProjectileLaunchSettings lightningLaunchSettings = new MagicProjectileLaunchSettings
    {
        projectileColor = new Color(1f, 0.95f, 0.25f, 0.9f),
        projectileSpeed = 120f,
        projectileLifetime = 4f,
        effectRadius = 1.5f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        effectLifetime = 3f,
        envelopePadding = 0.15f
    };
    public MagicProjectileLaunchSettings windLaunchSettings = new MagicProjectileLaunchSettings
    {
        projectileColor = new Color(0.65f, 1f, 0.75f, 0.45f),
        projectileSpeed = 75f,
        projectileLifetime = 6f,
        effectRadius = 4f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        effectLifetime = 4f,
        envelopePadding = 0.3f
    };
    public MagicProjectileLaunchSettings spaceLaunchSettings = new MagicProjectileLaunchSettings
    {
        projectileColor = new Color(0.75f, 0.45f, 1f, 0.7f),
        projectileSpeed = 50f,
        projectileLifetime = 7f,
        effectRadius = 2.5f,
        iceSpikeHeight = 0f,
        iceSpikeRadius = 0f,
        wrapNonTerrainTargets = false,
        effectLifetime = 5f,
        envelopePadding = 0.25f
    };
    void Start()
    {
        if (sourceCamera == null)
            sourceCamera = Camera.main;

        if (spellCircleSettings.radius <= 0f)
            spellCircleSettings = MagicCircleLaunchSettings.Default;
    }

    void Update()
    {
        if (sourceCamera == null)
            return;

        HandleElementHotkeys();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            AssignLaunchSettingsByElement();
            LaunchProjectile();
        }
    }

    static bool IsPointerOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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

    void LaunchProjectile()
    {
        if (isCharging)
            return;

        isCharging = true;
        Ray ray = sourceCamera.ScreenPointToRay(Input.mousePosition);
        MagicElement element = launchElement;
        MagicProjectileLaunchSettings launchSettings = currentLaunchSettings.WithFallbackEffectLifetime(effectLifetime);
        MagicLaunchApi.LaunchWithCharge(this, new MagicLaunchRequest
        {
            element = element,
            origin = ray.origin,
            direction = ray.direction,
            spawnOffset = projectileSpawnOffset,
            projectileScale = projectileScale,
            projectileSettings = launchSettings,
            spellCircleSettings = spellCircleSettings,
            projectileName = $"{element} Launched Projectile"
        });
        Invoke(nameof(ClearCharging), Mathf.Max(0.01f, spellCircleSettings.chargeDuration));
    }

    void ClearCharging()
    {
        isCharging = false;
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

}
