using UnityEngine;

[DefaultExecutionOrder(-23)]
public class WindFieldManager : MonoBehaviour
{
    public static WindFieldManager Instance { get; private set; }

    [Header("Grid")]
    public int gridResolution = 32;

    [Header("Simulation")]
    public float diffusionRate = 1.2f;
    public float decayRate = 0.2f;

    [Header("AI Bridge")]
    public field2AI field2AI;

    Terrain terrain;
    Vector2[,] windField;
    Vector2[,] nextWindField;
    float cellSizeX = 1f;
    float cellSizeZ = 1f;
    bool initialized;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveField2AI();
    }

    void Update()
    {
        EnsureInitialized();
        if (!initialized)
            return;

        Simulate(Time.deltaTime);
    }

    public static WindFieldManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        WindFieldManager existing = FindFirstObjectByType<WindFieldManager>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("WindFieldManager");
        return go.AddComponent<WindFieldManager>();
    }

    public void AddWind(Vector3 worldPosition, Vector3 worldDirection, float strength, float radius = 4f)
    {
        EnsureInitialized();
        if (!initialized || strength <= 0f)
            return;

        Vector2 direction = new Vector2(worldDirection.x, worldDirection.z);
        if (direction.sqrMagnitude <= 0.0001f)
            direction = Vector2.up;
        else
            direction.Normalize();

        WorldToGrid(worldPosition, out int centerX, out int centerZ);
        int radiusCellsX = Mathf.Max(1, Mathf.CeilToInt(radius / Mathf.Max(0.001f, cellSizeX)));
        int radiusCellsZ = Mathf.Max(1, Mathf.CeilToInt(radius / Mathf.Max(0.001f, cellSizeZ)));

        float totalWeight = 0f;
        for (int z = -radiusCellsZ; z <= radiusCellsZ; z++)
        {
            for (int x = -radiusCellsX; x <= radiusCellsX; x++)
            {
                int gx = centerX + x;
                int gz = centerZ + z;
                if (!IsInside(gx, gz))
                    continue;

                float dx = x * cellSizeX;
                float dz = z * cellSizeZ;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > radius)
                    continue;

                totalWeight += Mathf.Max(0.001f, 1f - dist / Mathf.Max(radius, 0.001f));
            }
        }

        if (totalWeight <= 0f)
            return;

        Vector2 wind = direction * strength;
        for (int z = -radiusCellsZ; z <= radiusCellsZ; z++)
        {
            for (int x = -radiusCellsX; x <= radiusCellsX; x++)
            {
                int gx = centerX + x;
                int gz = centerZ + z;
                if (!IsInside(gx, gz))
                    continue;

                float dx = x * cellSizeX;
                float dz = z * cellSizeZ;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist > radius)
                    continue;

                float weight = Mathf.Max(0.001f, 1f - dist / Mathf.Max(radius, 0.001f));
                windField[gx, gz] += wind * (weight / totalWeight);
            }
        }
    }

    public Vector2 SampleWind(Vector3 worldPosition)
    {
        EnsureInitialized();
        if (!initialized)
            return Vector2.zero;

        WorldToGrid(worldPosition, out int gx, out int gz);
        return IsInside(gx, gz) ? windField[gx, gz] : Vector2.zero;
    }

    void ResolveField2AI()
    {
        if (field2AI == null)
            field2AI = FindFirstObjectByType<field2AI>();
        if (field2AI != null)
            field2AI.windFieldManager = this;
    }

    public void ClearAllWind()
    {
        EnsureInitialized();
        if (!initialized)
            return;

        int width = windField.GetLength(0);
        int height = windField.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                windField[x, z] = Vector2.zero;
                nextWindField[x, z] = Vector2.zero;
            }
        }
    }

    void EnsureInitialized()
    {
        if (initialized)
            return;

        WorldGenerator world = FindFirstObjectByType<WorldGenerator>();
        if (world == null || world.terrain == null || world.terrain.terrainData == null)
            return;

        terrain = world.terrain;
        int size = Mathf.Max(8, gridResolution);
        windField = new Vector2[size, size];
        nextWindField = new Vector2[size, size];
        cellSizeX = terrain.terrainData.size.x / (size - 1);
        cellSizeZ = terrain.terrainData.size.z / (size - 1);
        initialized = true;
    }

    void Simulate(float dt)
    {
        int width = windField.GetLength(0);
        int height = windField.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2 center = windField[x, z];
                Vector2 neighborSum = GetWind(x - 1, z) + GetWind(x + 1, z) + GetWind(x, z - 1) + GetWind(x, z + 1);
                Vector2 neighborAvg = neighborSum * 0.25f;
                Vector2 diffused = center + (neighborAvg - center) * Mathf.Clamp01(diffusionRate * dt);
                nextWindField[x, z] = diffused * Mathf.Max(0f, 1f - decayRate * dt);
            }
        }

        var swap = windField;
        windField = nextWindField;
        nextWindField = swap;
    }

    Vector2 GetWind(int x, int z)
    {
        x = Mathf.Clamp(x, 0, windField.GetLength(0) - 1);
        z = Mathf.Clamp(z, 0, windField.GetLength(1) - 1);
        return windField[x, z];
    }

    bool IsInside(int x, int z)
    {
        return x >= 0 && x < windField.GetLength(0) && z >= 0 && z < windField.GetLength(1);
    }

    void WorldToGrid(Vector3 worldPosition, out int gx, out int gz)
    {
        Vector3 terrainPos = terrain != null ? terrain.transform.position : Vector3.zero;
        Vector3 size = terrain != null && terrain.terrainData != null ? terrain.terrainData.size : new Vector3(1f, 0f, 1f);

        float nx = Mathf.InverseLerp(terrainPos.x, terrainPos.x + size.x, worldPosition.x);
        float nz = Mathf.InverseLerp(terrainPos.z, terrainPos.z + size.z, worldPosition.z);
        gx = Mathf.RoundToInt(nx * (windField.GetLength(0) - 1));
        gz = Mathf.RoundToInt(nz * (windField.GetLength(1) - 1));
    }

}
