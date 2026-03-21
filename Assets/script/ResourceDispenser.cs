using UnityEngine;
using System.Collections.Generic;
using static Resource;

public class ResourceDispenser : MonoBehaviour
{
    public static ResourceDispenser Instance { get; private set; }

    public GameObject Worldgen;
    public GameObject herbivoreManager;
    public GameObject predatorManager;

    int grassIndex = 0;
    public float totalCarbon = 0f;
    [SerializeField] float carbonPool = 0f;

    public float carbonPerGrass = 30f;
    public float carbonRegenRate = 0.5f;

    public float carbonPerHerbivore = 100f;
    public float carbonPerPredator = 200f;

    [Header("Creature Energy/Life")]
    public float decomposeRate = 2f;
    public float carbonToEnergyRate = 0.5f;
    public float metabolicEnergyPerCarbon = 1f;
    public float metabolicHeatPerCarbon = 0.5f;
    public float idleEnergyCostPerSec = 0.05f;
    public float moveEnergyCostPerSec = 0.2f;
    public float accelerationEnergyCostPerUnit = 0.03f;
    public float brakingEnergyCostPerUnit = 0.02f;
    public float turnEnergyCostPerDegree = 0.0005f;
    public float decompositionHeatPerCarbon = 1f;

    [Header("Initial Spawn")]
    public int initialGrassCount = 100;
    public int initialHerbivoreCount = 30;
    public int initialPredatorCount = 10;

    [Header("Count Per Gen")]
    public int grassCountPerGeneration = 100;
    public int herbivoreCountPerGeneration = 30;
    public int predatorCountPerGeneration = 10;

    [Header("Spawn Safety")]
    public int maxSpawnAttemptsPerEntity = 64;

    public List<GameObject> grasses;
    float nextCarbonAuditTime = 60f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        grasses = new List<GameObject>();
        grassIndex = 0;
        ResetGenerationCarbonState();
    }

    void Initialspown()
    {
        ConfigureCarbonBudget(initialGrassCount, initialHerbivoreCount, initialPredatorCount);
        SpawnGrassCount(initialGrassCount);

        var hm = herbivoreManager.GetComponent<herbivoreManager>();
        int herbivoreSpawned = TrySpawnHerbivoreBatch(hm, initialHerbivoreCount, 0);

        var pm = predatorManager.GetComponent<predatorManager>();
        int predatorSpawned = TrySpawnPredatorBatch(pm, initialPredatorCount, 0);

        LogSpawnShortfall("initial herbivore", herbivoreSpawned, initialHerbivoreCount);
        LogSpawnShortfall("initial predator", predatorSpawned, initialPredatorCount);
    }

    void ResouseInit(GameObject obj, float amount, category category)
    {
        if (obj == null) return;

        var comp = obj.GetComponent<Resource>();
        if (comp == null)
            comp = obj.AddComponent<Resource>();

        comp.InitCarbon(amount, amount);
        comp.resourceCategory = category;
    }

    public void InitializeCreatureResource(GameObject obj, float amount, category category)
    {
        if (obj == null) return;

        var comp = obj.GetComponent<Resource>();
        if (comp == null)
            comp = obj.AddComponent<Resource>();

        comp.InitCarbon(amount, amount);
        comp.resourceCategory = category;
    }

    public void AddExternalCarbon(float amount)
    {
        if (amount <= 0f)
            return;

        totalCarbon += amount;
    }

    public void ReturnCarbon(float amount)
    {
        if (amount <= 0f) return;
        carbonPool += amount;
        if (totalCarbon > 0f)
            carbonPool = Mathf.Min(carbonPool, totalCarbon);
    }

    public bool Addgrass()
    {
        grassIndex++;
        if (spowngrass(grassIndex, out GameObject grass))
        {
            grasses.Add(grass);
            ResouseInit(grass, carbonPerGrass, category.grass);
            return true;
        }

        return false;
    }

    public void SpawnGrassCount(int count)
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = GetMaxAttempts(count);
        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            if (Addgrass())
                spawned++;
        }

        LogSpawnShortfall("grass", spawned, count);
    }

    bool issetting = false;
    void Update()
    {
        if (!issetting && Worldgen.GetComponent<WorldGenerator>().isgenerating)
        {
            Initialspown();
            issetting = true;
        }

        for (int i = grasses.Count - 1; i >= 0; i--)
        {
            GameObject grass = grasses[i];
            if (grass == null)
            {
                grasses.RemoveAt(i);
                continue;
            }

            var resource = grass.GetComponent<Resource>();
            if (resource == null) continue;

            float regenRequest = Mathf.Max(0f, carbonRegenRate) * Time.deltaTime;
            if (regenRequest <= 0f || carbonPool <= 0f) continue;

            float regenAmount = Mathf.Min(regenRequest, carbonPool);
            resource.AddCarbon(regenAmount, out float excess);
            float added = regenAmount - excess;
            if (added > 0f)
                carbonPool -= added;
        }

        if (Time.time >= nextCarbonAuditTime)
        {
            LogCarbonAudit();
            nextCarbonAuditTime += 60f;
        }
    }

    void LogCarbonAudit()
    {
        Resource[] resources = FindObjectsByType<Resource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float sum = 0f;
        float grassCarbon = 0f;
        float herbivoreCarbon = 0f;
        float predatorCarbon = 0f;

        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i] == null) continue;
            float c = resources[i].carbon;
            sum += c;
            switch (resources[i].resourceCategory)
            {
                case category.grass:
                    grassCarbon += c;
                    break;
                case category.herbivore:
                    herbivoreCarbon += c;
                    break;
                case category.predator:
                    predatorCarbon += c;
                    break;
            }
        }

        float systemTotal = sum + carbonPool;
        float observedMinusConfigured = systemTotal - totalCarbon;
        string msg = $"[CarbonAudit] t={Time.time:F1}s totalResourceCarbon={sum:F3} poolCarbon={carbonPool:F3} systemTotal={systemTotal:F3} observedMinusConfigured={observedMinusConfigured:F3} resourceCount={resources.Length} grass={grassCarbon:F3} herbivore={herbivoreCarbon:F3} predator={predatorCarbon:F3} configuredTotalCarbon={totalCarbon:F3}";
        Debug.Log(msg);
    }

    void ResetCarbonBudget()
    {
        carbonPool = Mathf.Max(0f, totalCarbon);
    }

    public void ResetGenerationCarbonState()
    {
        carbonPool = 0f;
        totalCarbon = 0f;
    }

    public void FinalizeGenerationCarbonBudget()
    {
        totalCarbon = Mathf.Max(0f, totalCarbon);
    }

    public void ConfigureCarbonBudget(int grassCount, int herbivoreCount, int predatorCount)
    {
        totalCarbon =
            Mathf.Max(0, grassCount) * Mathf.Max(0f, carbonPerGrass) +
            Mathf.Max(0, herbivoreCount) * Mathf.Max(0f, carbonPerHerbivore) +
            Mathf.Max(0, predatorCount) * Mathf.Max(0f, carbonPerPredator);
        carbonPool = 0f;
    }

    int TrySpawnHerbivoreBatch(herbivoreManager manager, int count, int startIndex)
    {
        if (manager == null)
            return 0;

        int spawned = 0;
        int attempts = 0;
        int nextIndex = startIndex;
        int maxAttempts = GetMaxAttempts(count);
        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            if (manager.spownherbivore(Worldgen, nextIndex++, out GameObject herbivore))
            {
                ResouseInit(herbivore, carbonPerHerbivore, category.herbivore);
                spawned++;
            }
        }

        return spawned;
    }

    int TrySpawnPredatorBatch(predatorManager manager, int count, int startIndex)
    {
        if (manager == null)
            return 0;

        int spawned = 0;
        int attempts = 0;
        int nextIndex = startIndex;
        int maxAttempts = GetMaxAttempts(count);
        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            if (manager.spownpredator(Worldgen, nextIndex++, out GameObject predator))
            {
                ResouseInit(predator, carbonPerPredator, category.predator);
                spawned++;
            }
        }

        return spawned;
    }

    int GetMaxAttempts(int targetCount)
    {
        int safeTarget = Mathf.Max(0, targetCount);
        int perEntity = Mathf.Max(1, maxSpawnAttemptsPerEntity);
        return Mathf.Max(1, safeTarget * perEntity);
    }

    void LogSpawnShortfall(string label, int spawned, int target)
    {
        if (spawned >= target)
            return;

        Debug.LogWarning($"[SpawnBudget] Could not reach target {label} count. spawned={spawned} target={target}");
    }

    public void ResetGenerationEnvironment()
    {
        ClearGrasslands();
        ResetGenerationCarbonState();
        HeatFieldManager.GetOrCreate().ClearAllHeat();
        ConfigureCarbonBudget(grassCountPerGeneration, herbivoreCountPerGeneration, predatorCountPerGeneration);
        SpawnGrassCount(grassCountPerGeneration);
    }

    public void ClearGrasslands()
    {
        if (grasses == null)
            grasses = new List<GameObject>();

        for (int i = grasses.Count - 1; i >= 0; i--)
        {
            GameObject grass = grasses[i];
            if (grass != null)
                Destroy(grass);
        }

        grasses.Clear();
        grassIndex = 0;
    }

    [Header("Vegetation")]
    public float radius = 20f;
    public float density = 0.2f;
    public int plantCount = 20;
    public float maxSlope = 30f;

    bool spowngrass(int index, out GameObject grassland)
    {
        grassland = null;

        WorldGenerator wg = Worldgen.GetComponent<WorldGenerator>();
        TerrainData data = wg.terrain.terrainData;

        var rng = new System.Random(wg.seed + index);
        int maxTry = 10;

        for (int t = 0; t < maxTry; t++)
        {
            float centerX = (float)rng.NextDouble() * wg.terrainSize;
            float centerZ = (float)rng.NextDouble() * wg.terrainSize;

            float normX = (centerX - wg.terrain.transform.position.x) / data.size.x;
            float normZ = (centerZ - wg.terrain.transform.position.z) / data.size.z;

            float height = data.GetInterpolatedHeight(normX, normZ);
            if (height <= wg.waterHeight) continue;

            Vector3 spawnPos = new Vector3(centerX, height, centerZ) + new Vector3(0, 4f, 0);

            grassland = new GameObject("Grassland_Meadow_" + index);
            grassland.transform.position = spawnPos;
            grassland.transform.parent = this.transform;
            grassland.layer = LayerMask.NameToLayer("Grassland");

            var gl = grassland.AddComponent<grassland>();
            gl.tdata = data;
            gl.setting(wg.waterHeight, plantCount, radius, density, maxSlope);
            gl.setseed(rng);
            gl.ready();

            return true;
        }

        return false;
    }
}

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
        if (!initialized || amount <= 0f)
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
                nextHeatField[x, z] = Mathf.Max(0f, decayed);
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

