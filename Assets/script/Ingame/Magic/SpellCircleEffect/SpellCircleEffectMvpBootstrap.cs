using UnityEngine;

public class SpellCircleEffectMvpBootstrap : MonoBehaviour
{
    public int terrainSize = 250;
    public int heightmapResolution = 250;
    public float heightScale = 20f;
    public float waterHeight = 4f;
    public MagicElement initialElement = MagicElement.Ice;

    void Awake()
    {
        WorldGenerator worldGenerator = EnsureWorldGenerator();
        Camera camera = EnsureCamera(worldGenerator);
        EnsureLight();
        EnsureExperimentRig(camera, worldGenerator);
    }

    WorldGenerator EnsureWorldGenerator()
    {
        WorldGenerator existing = FindFirstObjectByType<WorldGenerator>();
        if (existing != null)
            return existing;

        GameObject worldObject = new GameObject("WorldGenerator");
        WorldGenerator worldGenerator = worldObject.AddComponent<WorldGenerator>();
        worldGenerator.seed = 13125;
        worldGenerator.terrainSize = terrainSize;
        worldGenerator.heightmapResolution = heightmapResolution;
        worldGenerator.heightScale = heightScale;
        worldGenerator.noiseScale = 0.1f;
        worldGenerator.octaves = 6;
        worldGenerator.persistence = 0.3f;
        worldGenerator.lacunarity = 0.6f;
        worldGenerator.waterHeight = waterHeight;
        return worldGenerator;
    }

    Camera EnsureCamera(WorldGenerator worldGenerator)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.transform.position = new Vector3(terrainSize * 0.5f, heightScale + 2f, terrainSize * 0.5f - 18f);
        camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        camera.fieldOfView = 62f;

        FreeFlyCamera freeFly = camera.GetComponent<FreeFlyCamera>();
        if (freeFly == null)
        {
            bool wasActive = camera.gameObject.activeSelf;
            camera.gameObject.SetActive(false);
            freeFly = camera.gameObject.AddComponent<FreeFlyCamera>();
            freeFly.worldgen = worldGenerator.gameObject;
            camera.gameObject.SetActive(wasActive);
        }

        freeFly.worldgen = worldGenerator.gameObject;
        freeFly.moveSpeed = 14f;
        freeFly.scrollSpeed = 50f;
        freeFly.groundOffset = 1.2f;
        camera.transform.position = new Vector3(terrainSize * 0.5f, heightScale + 2f, terrainSize * 0.5f - 18f);
        camera.transform.rotation = Quaternion.Euler(18f, 0f, 0f);
        freeFly.SyncRotationFromTransform();
        return camera;
    }

    void EnsureLight()
    {
        if (FindFirstObjectByType<Light>() != null)
            return;

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.84f, 1f);
        light.intensity = 1f;
    }

    void EnsureExperimentRig(Camera camera, WorldGenerator worldGenerator)
    {
        GameObject rig = GameObject.Find("Spell Circle Effect MVP");
        if (rig == null)
            rig = new GameObject("Spell Circle Effect MVP");

        MagicMaterialExperimentLauncher launcher = rig.GetComponent<MagicMaterialExperimentLauncher>();
        if (launcher == null)
            launcher = rig.AddComponent<MagicMaterialExperimentLauncher>();

        launcher.sourceCamera = camera;
        launcher.launchElement = initialElement;
        launcher.spellCircleSettings = MagicCircleLaunchSettings.Default;

        MagicExperimentTargetSpawner targetSpawner = rig.GetComponent<MagicExperimentTargetSpawner>();
        if (targetSpawner == null)
            targetSpawner = rig.AddComponent<MagicExperimentTargetSpawner>();

        targetSpawner.sourceCamera = camera;
        targetSpawner.worldGenerator = worldGenerator;
        targetSpawner.targetCount = 7;
        targetSpawner.spawnRadius = 26f;
        targetSpawner.spawnRadiusJitter = 6f;
    }
}
