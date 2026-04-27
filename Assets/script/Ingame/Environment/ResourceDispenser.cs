using UnityEngine;
using System.Collections.Generic;
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

