using UnityEngine;

[DefaultExecutionOrder(-24)]
public class ManaFieldManager : MonoBehaviour
{
    public static ManaFieldManager Instance { get; private set; }

    [Header("Grid")]
    public int gridResolution = 96;

    [Header("Simulation")]
    public float diffusionRate = 1.5f;
    public bool debugDrawMana = true;
    public float debugBaseHeight = 22f;
    public float debugManaScale = 1f;

    Terrain terrain;
    float[,] manaField;
    float[,] nextManaField;
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
    }

    void Update()
    {
        EnsureInitialized();
        if (!initialized)
            return;

        Simulate(Time.deltaTime);
        DrawDebugManaGrid();
    }

    public static ManaFieldManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        ManaFieldManager existing = FindFirstObjectByType<ManaFieldManager>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("ManaFieldManager");
        return go.AddComponent<ManaFieldManager>();
    }

    public void AddMana(Vector3 worldPosition, float amount, float radius = 2f)
    {
        EnsureInitialized();
        if (!initialized || amount <= 0f)
            return;

        AddManaInternal(worldPosition, amount, radius);
    }

    public bool TrySpendMana(Vector3 worldPosition, float amount, float radius = 2f)
    {
        EnsureInitialized();
        if (!initialized || amount <= 0f)
            return false;

        float available = SampleMana(worldPosition, radius);
        if (available < amount)
            return false;

        AddManaInternal(worldPosition, -amount, radius);
        return true;
    }

    public float SampleMana(Vector3 worldPosition)
    {
        EnsureInitialized();
        if (!initialized)
            return 0f;

        WorldToGrid(worldPosition, out int gx, out int gz);
        return IsInside(gx, gz) ? manaField[gx, gz] : 0f;
    }

    public float SampleMana(Vector3 worldPosition, float radius)
    {
        EnsureInitialized();
        if (!initialized)
            return 0f;

        WorldToGrid(worldPosition, out int centerX, out int centerZ);
        int radiusCellsX = Mathf.Max(1, Mathf.CeilToInt(radius / Mathf.Max(0.001f, cellSizeX)));
        int radiusCellsZ = Mathf.Max(1, Mathf.CeilToInt(radius / Mathf.Max(0.001f, cellSizeZ)));

        float total = 0f;
        for (int z = -radiusCellsZ; z <= radiusCellsZ; z++)
        {
            for (int x = -radiusCellsX; x <= radiusCellsX; x++)
            {
                int gx = centerX + x;
                int gz = centerZ + z;
                if (IsInside(gx, gz))
                    total += manaField[gx, gz];
            }
        }

        return total;
    }

    public Vector2 SampleManaFlow(Vector3 worldPosition)
    {
        EnsureInitialized();
        if (!initialized)
            return Vector2.zero;

        WorldToGrid(worldPosition, out int gx, out int gz);
        float left = GetMana(gx - 1, gz);
        float right = GetMana(gx + 1, gz);
        float down = GetMana(gx, gz - 1);
        float up = GetMana(gx, gz + 1);
        return new Vector2(right - left, up - down);
    }

    public void ClearAllMana()
    {
        EnsureInitialized();
        if (!initialized)
            return;

        int width = manaField.GetLength(0);
        int height = manaField.GetLength(1);
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                manaField[x, z] = 0f;
                nextManaField[x, z] = 0f;
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
        manaField = new float[size, size];
        nextManaField = new float[size, size];
        cellSizeX = terrain.terrainData.size.x / (size - 1);
        cellSizeZ = terrain.terrainData.size.z / (size - 1);
        initialized = true;
    }

    void AddManaInternal(Vector3 worldPosition, float amount, float radius)
    {
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
                manaField[gx, gz] = Mathf.Max(0f, manaField[gx, gz] + amount * (weight / totalWeight));
            }
        }
    }

    void Simulate(float dt)
    {
        int width = manaField.GetLength(0);
        int height = manaField.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float center = manaField[x, z];
                float neighborSum = GetMana(x - 1, z) + GetMana(x + 1, z) + GetMana(x, z - 1) + GetMana(x, z + 1);
                float neighborAvg = neighborSum * 0.25f;
                nextManaField[x, z] = Mathf.Max(0f, center + (neighborAvg - center) * Mathf.Clamp01(diffusionRate * dt));
            }
        }

        var swap = manaField;
        manaField = nextManaField;
        nextManaField = swap;
    }

    float GetMana(int x, int z)
    {
        x = Mathf.Clamp(x, 0, manaField.GetLength(0) - 1);
        z = Mathf.Clamp(z, 0, manaField.GetLength(1) - 1);
        return manaField[x, z];
    }

    bool IsInside(int x, int z)
    {
        return x >= 0 && x < manaField.GetLength(0) && z >= 0 && z < manaField.GetLength(1);
    }

    void WorldToGrid(Vector3 worldPosition, out int gx, out int gz)
    {
        Vector3 terrainPos = terrain != null ? terrain.transform.position : Vector3.zero;
        Vector3 size = terrain != null && terrain.terrainData != null ? terrain.terrainData.size : new Vector3(1f, 0f, 1f);

        float nx = Mathf.InverseLerp(terrainPos.x, terrainPos.x + size.x, worldPosition.x);
        float nz = Mathf.InverseLerp(terrainPos.z, terrainPos.z + size.z, worldPosition.z);
        gx = Mathf.RoundToInt(nx * (manaField.GetLength(0) - 1));
        gz = Mathf.RoundToInt(nz * (manaField.GetLength(1) - 1));
    }

    void DrawDebugManaGrid()
    {
        if (!debugDrawMana || !initialized || terrain == null)
            return;

        Vector3 terrainPos = terrain.transform.position;
        int width = manaField.GetLength(0);
        int height = manaField.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float mana = manaField[x, z];
                Vector3 start = new Vector3(
                    terrainPos.x + x * cellSizeX,
                    debugBaseHeight,
                    terrainPos.z + z * cellSizeZ);
                Vector3 end = start + Vector3.up * (mana * debugManaScale);
                Debug.DrawLine(start, end, Color.cyan, 0f, false);
            }
        }
    }
}
