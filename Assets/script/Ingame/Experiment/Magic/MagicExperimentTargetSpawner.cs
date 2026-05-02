using System.Collections;
using UnityEngine;

public class MagicExperimentTargetSpawner : MonoBehaviour
{
    public Camera sourceCamera;
    public WorldGenerator worldGenerator;
    public int targetCount = 6;
    public int randomSeed = 1234;
    public float spawnRadius = 22f;
    public float spawnRadiusJitter = 4f;
    public Vector3 minTargetSize = new Vector3(2f, 2f, 0.8f);
    public Vector3 maxTargetSize = new Vector3(5f, 5f, 1.4f);
    public bool avoidWater = true;
    public bool alignToTerrainNormal = true;
    public float groundClearance = 0.05f;
    public int maxPlacementAttempts = 16;

    IEnumerator Start()
    {
        if (sourceCamera == null)
            sourceCamera = Camera.main;

        if (worldGenerator == null)
            worldGenerator = FindFirstObjectByType<WorldGenerator>();

        while (worldGenerator != null && worldGenerator.terrain == null)
            yield return null;

        SpawnTargets();
    }

    public void SpawnTargets()
    {
        if (sourceCamera == null)
            return;

        if (GameObject.Find("Magic Experiment Target 00") != null)
            return;

        Random.InitState(randomSeed);

        for (int i = 0; i < Mathf.Max(0, targetCount); i++)
        {
            Vector3 scale = GetRandomScale();
            Vector3 position = GetRandomSpawnPosition(scale);
            Vector3 normal = SampleTerrainNormal(position);

            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = $"Magic Experiment Target {i:00}";
            target.transform.position = position;
            target.transform.localScale = scale;
            target.transform.rotation = CreateTerrainAlignedRotation(normal);

            var experimentTarget = target.AddComponent<MagicExperimentTarget>();
            experimentTarget.Initialize(i, normal, scale, alignToTerrainNormal);

            var renderer = target.GetComponent<Renderer>();
            renderer.material = CreateTargetMaterial(i);
        }
    }

    Vector3 GetRandomSpawnPosition(Vector3 targetScale)
    {
        Terrain terrain = worldGenerator != null ? worldGenerator.terrain : Terrain.activeTerrain;
        if (terrain == null)
            return GetCameraRelativeFallbackPosition();

        int attempts = Mathf.Max(1, maxPlacementAttempts);
        Vector3 position = Vector3.zero;
        for (int i = 0; i < attempts; i++)
        {
            position = ClampToTerrainBounds(GetCameraRelativeGroundPoint(), terrain);
            float groundHeight = terrain.SampleHeight(position) + terrain.transform.position.y;
            if (!avoidWater || worldGenerator == null || groundHeight > worldGenerator.waterHeight)
            {
                position.y = groundHeight + targetScale.y * 0.5f + groundClearance;
                return position;
            }
        }

        float fallbackHeight = terrain.SampleHeight(position) + terrain.transform.position.y;
        position.y = fallbackHeight + targetScale.y * 0.5f + groundClearance;
        return position;
    }

    Vector3 GetRandomScale()
    {
        return new Vector3(
            Random.Range(minTargetSize.x, maxTargetSize.x),
            Random.Range(minTargetSize.y, maxTargetSize.y),
            Random.Range(minTargetSize.z, maxTargetSize.z));
    }

    Vector3 GetCameraRelativeGroundPoint()
    {
        Transform cam = sourceCamera.transform;
        float angle = Random.value * Mathf.PI * 2f;
        float radius = Mathf.Max(0f, spawnRadius + Random.Range(-spawnRadiusJitter, spawnRadiusJitter));
        Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        return cam.position + radial * radius;
    }

    Vector3 GetCameraRelativeFallbackPosition()
    {
        Transform cam = sourceCamera.transform;
        float angle = Random.value * Mathf.PI * 2f;
        float radius = Mathf.Max(0f, spawnRadius + Random.Range(-spawnRadiusJitter, spawnRadiusJitter));
        return cam.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
    }

    static Vector3 ClampToTerrainBounds(Vector3 position, Terrain terrain)
    {
        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        position.x = Mathf.Clamp(position.x, origin.x, origin.x + size.x);
        position.z = Mathf.Clamp(position.z, origin.z, origin.z + size.z);
        return position;
    }

    Vector3 SampleTerrainNormal(Vector3 position)
    {
        Terrain terrain = worldGenerator != null ? worldGenerator.terrain : Terrain.activeTerrain;
        if (terrain == null)
            return Vector3.up;

        Vector3 origin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        float normalizedX = Mathf.InverseLerp(origin.x, origin.x + size.x, position.x);
        float normalizedZ = Mathf.InverseLerp(origin.z, origin.z + size.z, position.z);
        return terrain.terrainData.GetInterpolatedNormal(normalizedX, normalizedZ).normalized;
    }

    Quaternion CreateTerrainAlignedRotation(Vector3 normal)
    {
        if (!alignToTerrainNormal)
            normal = Vector3.up;

        Vector3 forward = Vector3.ProjectOnPlane(sourceCamera.transform.forward, normal);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.ProjectOnPlane(Vector3.forward, normal);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.Cross(normal, Vector3.right);

        return Quaternion.LookRotation(forward.normalized, normal);
    }

    static Material CreateTargetMaterial(int index)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color color = Color.HSVToRGB(Mathf.Repeat(0.53f + index * 0.06f, 1f), 0.55f, 0.95f);
        color.a = 1f;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.65f);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 0f);
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        return material;
    }
}
