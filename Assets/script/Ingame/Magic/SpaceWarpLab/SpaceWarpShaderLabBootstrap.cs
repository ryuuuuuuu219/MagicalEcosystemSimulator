using UnityEngine;

public class SpaceWarpShaderLabBootstrap : MonoBehaviour
{
    public int terrainSize = 180;
    public int heightmapResolution = 180;
    public float heightScale = 14f;
    public float waterHeight = 3.5f;
    public Vector3 cameraPosition = new Vector3(100f, 50f, 80f);
    public Vector3 warpSourcePoint = new Vector3(80f, 30f, 90f);
    public Vector3 warpDestinationPoint = new Vector3(100f, 30f, 100f);
    public float warpRadius = 5f;
    public float warpLifetime = 30f;

    void Awake()
    {
        WorldGenerator worldGenerator = EnsureWorldGenerator();
        Camera camera = EnsureCamera(worldGenerator);
        EnsureLight();
        EnsureBackdrop();
        EnsureWarpPreview(camera);
    }

    WorldGenerator EnsureWorldGenerator()
    {
        WorldGenerator existing = FindFirstObjectByType<WorldGenerator>();
        if (existing != null)
            return existing;

        GameObject worldObject = new GameObject("WorldGenerator");
        WorldGenerator worldGenerator = worldObject.AddComponent<WorldGenerator>();
        worldGenerator.seed = 260507;
        worldGenerator.terrainSize = terrainSize;
        worldGenerator.heightmapResolution = heightmapResolution;
        worldGenerator.heightScale = heightScale;
        worldGenerator.noiseScale = 0.08f;
        worldGenerator.octaves = 5;
        worldGenerator.persistence = 0.35f;
        worldGenerator.lacunarity = 0.7f;
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

        Vector3 position = cameraPosition;
        Quaternion rotation = Quaternion.Euler(45f, 0f, 0f);
        camera.transform.SetPositionAndRotation(position, rotation);
        camera.fieldOfView = 60f;
        camera.clearFlags = CameraClearFlags.Skybox;

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
        freeFly.moveSpeed = 12f;
        freeFly.scrollSpeed = 45f;
        freeFly.groundOffset = 1.2f;
        camera.transform.SetPositionAndRotation(position, rotation);
        freeFly.SyncRotationFromTransform();
        return camera;
    }

    void EnsureLight()
    {
        if (FindFirstObjectByType<Light>() != null)
            return;

        GameObject lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.97f, 0.9f, 1f);
        light.intensity = 1.15f;
    }

    void EnsureBackdrop()
    {
        if (GameObject.Find("Space Warp Lab Backdrop") != null)
            return;

        GameObject root = new GameObject("Space Warp Lab Backdrop");
        Material[] materials =
        {
            CreateOpaqueMaterial("Lab Coral", new Color(0.95f, 0.32f, 0.38f, 1f)),
            CreateOpaqueMaterial("Lab Cyan", new Color(0.12f, 0.72f, 0.84f, 1f)),
            CreateOpaqueMaterial("Lab Amber", new Color(1f, 0.78f, 0.22f, 1f)),
            CreateOpaqueMaterial("Lab Violet", new Color(0.54f, 0.42f, 0.95f, 1f))
        };

        Vector3 center = new Vector3(terrainSize * 0.5f, heightScale + 3.5f, terrainSize * 0.5f + 8f);
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                GameObject marker = GameObject.CreatePrimitive((x + y) % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                marker.name = $"Warp Reference {x:+0;-0;0},{y:+0;-0;0}";
                marker.transform.SetParent(root.transform, false);
                marker.transform.position = center + new Vector3(x * 4f, y * 2.4f, 14f + Mathf.Abs(x) * 0.4f);
                marker.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.5f, Mathf.Abs(y) / 2f);
                marker.GetComponent<Renderer>().material = materials[Mathf.Abs(x + y) % materials.Length];
            }
        }
    }

    void EnsureWarpPreview(Camera camera)
    {
        if (GameObject.Find("Space Warp Source") != null || GameObject.Find("Space Warp Destination") != null)
            return;

        SpaceWarpPortalPair pair = SpaceWarpApi.CreateTeleportWarp(
            warpSourcePoint,
            warpDestinationPoint,
            warpRadius,
            warpLifetime);

        NotifyIfWarpPairOutsideView(camera, pair);
    }

    static Material CreateOpaqueMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = name;
        material.color = color;
        return material;
    }

    Vector3 ResolveCameraFocusPoint()
    {
        return Vector3.Lerp(warpSourcePoint, warpDestinationPoint, 0.5f);
    }

    static void NotifyIfWarpPairOutsideView(Camera camera, SpaceWarpPortalPair pair)
    {
        if (camera == null)
            return;

        bool sourceVisible = IsRendererFullyInView(camera, pair.sourceWarp);
        bool destinationVisible = IsRendererFullyInView(camera, pair.destinationWarp);
        if (sourceVisible && destinationVisible)
            return;

        Debug.LogWarning(
            $"SpaceWarpApi lab placement is not fully in view. sourceVisible={sourceVisible}, destinationVisible={destinationVisible}");
    }

    static bool IsRendererFullyInView(Camera camera, GameObject target)
    {
        if (camera == null || target == null)
            return false;

        Renderer renderer = target.GetComponentInChildren<Renderer>();
        if (renderer == null)
            return false;

        Bounds bounds = renderer.bounds;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 viewport = camera.WorldToViewportPoint(corners[i]);
            if (viewport.z < camera.nearClipPlane ||
                viewport.x < 0f || viewport.x > 1f ||
                viewport.y < 0f || viewport.y > 1f)
            {
                return false;
            }
        }

        return true;
    }

}
