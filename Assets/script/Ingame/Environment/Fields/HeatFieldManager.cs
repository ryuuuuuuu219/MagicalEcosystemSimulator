using UnityEngine;

[DefaultExecutionOrder(-25)]
public class HeatFieldManager : MonoBehaviour
{
    public static HeatFieldManager Instance { get; private set; }

    [Header("Grid")]
    public int gridResolution = 96;

    [Header("Simulation")]
    public float diffusionRate = 2.5f;
    public float decayRate = 0.08f;
    public bool debugDrawHeat = true;
    public float debugBaseHeight = 20f;
    public float debugHeatScale = 1f;

    [Header("AI Bridge")]
    public field2AI field2AI;

    Terrain terrain;
    float[,] heatField;
    float[,] nextHeatField;
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
        DrawDebugHeatGrid();
    }

    public static HeatFieldManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        HeatFieldManager existing = FindFirstObjectByType<HeatFieldManager>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("HeatFieldManager");
        return go.AddComponent<HeatFieldManager>();
    }

    public void AddHeat(Vector3 worldPosition, float amount, float radius = 2f)
    {
        EnsureInitialized();
        if (!initialized || Mathf.Approximately(amount, 0f))
            return;

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
                heatField[gx, gz] += amount * (weight / totalWeight);
            }
        }
    }

    public float SampleHeat(Vector3 worldPosition)
    {
        EnsureInitialized();
        if (!initialized)
            return 0f;

        WorldToGrid(worldPosition, out int gx, out int gz);
        return IsInside(gx, gz) ? heatField[gx, gz] : 0f;
    }

    void ResolveField2AI()
    {
        if (field2AI == null)
            field2AI = FindFirstObjectByType<field2AI>();
        if (field2AI != null)
            field2AI.heatFieldManager = this;
    }

    public void ClearAllHeat()
    {
        EnsureInitialized();
        if (!initialized)
            return;

        int width = heatField.GetLength(0);
        int height = heatField.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                heatField[x, z] = 0f;
                nextHeatField[x, z] = 0f;
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
        heatField = new float[size, size];
        nextHeatField = new float[size, size];
        cellSizeX = terrain.terrainData.size.x / (size - 1);
        cellSizeZ = terrain.terrainData.size.z / (size - 1);
        initialized = true;
    }

    void Simulate(float dt)
    {
        int width = heatField.GetLength(0);
        int height = heatField.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float center = heatField[x, z];
                float neighborSum = GetHeat(x - 1, z) + GetHeat(x + 1, z) + GetHeat(x, z - 1) + GetHeat(x, z + 1);
                float neighborAvg = neighborSum * 0.25f;
                float diffused = center + (neighborAvg - center) * Mathf.Clamp01(diffusionRate * dt);
                float decayed = diffused * Mathf.Max(0f, 1f - decayRate * dt);
                nextHeatField[x, z] = decayed;
            }
        }

        var swap = heatField;
        heatField = nextHeatField;
        nextHeatField = swap;
    }

    float GetHeat(int x, int z)
    {
        x = Mathf.Clamp(x, 0, heatField.GetLength(0) - 1);
        z = Mathf.Clamp(z, 0, heatField.GetLength(1) - 1);
        return heatField[x, z];
    }

    bool IsInside(int x, int z)
    {
        return x >= 0 && x < heatField.GetLength(0) && z >= 0 && z < heatField.GetLength(1);
    }

    void WorldToGrid(Vector3 worldPosition, out int gx, out int gz)
    {
        Vector3 terrainPos = terrain != null ? terrain.transform.position : Vector3.zero;
        Vector3 size = terrain != null && terrain.terrainData != null ? terrain.terrainData.size : new Vector3(1f, 0f, 1f);

        float nx = Mathf.InverseLerp(terrainPos.x, terrainPos.x + size.x, worldPosition.x);
        float nz = Mathf.InverseLerp(terrainPos.z, terrainPos.z + size.z, worldPosition.z);
        gx = Mathf.RoundToInt(nx * (heatField.GetLength(0) - 1));
        gz = Mathf.RoundToInt(nz * (heatField.GetLength(1) - 1));
    }

    void DrawDebugHeatGrid()
    {
        if (!debugDrawHeat || !initialized || terrain == null)
            return;

        Vector3 terrainPos = terrain.transform.position;
        int width = heatField.GetLength(0);
        int height = heatField.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float heat = heatField[x, z];
                Vector3 start = new Vector3(
                    terrainPos.x + x * cellSizeX,
                    debugBaseHeight,
                    terrainPos.z + z * cellSizeZ);
                Vector3 end = start + Vector3.up * (heat * debugHeatScale);
                Debug.DrawLine(start, end, Color.red, 0f, false);
            }
        }
    }
}


