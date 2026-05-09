using UnityEngine;
using System.Collections.Generic;
public class ResourceDispenser : MonoBehaviour
{
    public static ResourceDispenser Instance { get; private set; }

    public GameObject Worldgen;
    public GameObject herbivoreManager;
    public GameObject predatorManager;

    int grassIndex = 0;
    public float totalMana = 0f;
    [SerializeField] float manaPool = 0f;

    public float manaPerGrass = 30f;
    public float grassEatAmountPerTouch = 30f;
    public float grassSpawnCooldown = 10f;

    public float manaPerHerbivore = 100f;
    public float manaPerPredator = 200f;

    [Header("Creature Mana/Life")]
    public float decomposeRate = 2f;

    [Header("Initial Spawn")]
    public int initialGrassCount = 100;
    public int initialHerbivoreCount = 30;
    public int initialPredatorCount = 10;

    [Header("Count Per Gen")]
    public int grassCountPerGeneration = 100;
    public int herbivoreCountPerGeneration = 30;
    public int predatorCountPerGeneration = 10;
    public int highPredatorCountPerGeneration = 0;
    public int dominantCountPerGeneration = 0;

    [Header("Spawn Safety")]
    public int maxSpawnAttemptsPerEntity = 64;

    public List<GameObject> grasses;
    float nextManaAuditTime = 60f;
    int pendingGrassRespawns;
    float nextGrassRespawnTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        grasses = new List<GameObject>();
        grassIndex = 0;
        ResetGenerationManaState();
    }

    void Initialspown()
    {
        ConfigureManaBudget(initialGrassCount, initialHerbivoreCount, initialPredatorCount);
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

        comp.InitMana(amount, amount);
        comp.resourceCategory = category;
    }

    public void InitializeCreatureResource(GameObject obj, float amount, category category)
    {
        if (obj == null) return;

        var comp = obj.GetComponent<Resource>();
        if (comp == null)
            comp = obj.AddComponent<Resource>();

        comp.InitMana(amount, amount);
        comp.resourceCategory = category;
    }

    public void AddExternalMana(float amount)
    {
        if (amount <= 0f)
            return;

        totalMana += amount;
    }

    public void ReturnMana(float amount)
    {
        if (amount <= 0f) return;
        manaPool += amount;
        if (totalMana > 0f)
            manaPool = Mathf.Min(manaPool, totalMana);
    }

    public bool Addgrass()
    {
        grassIndex++;
        if (spowngrass(grassIndex, out GameObject grass))
        {
            grasses.Add(grass);
            ResouseInit(grass, manaPerGrass, category.grass);
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

        PruneGrassList();
        ProcessGrassRespawns();

        if (Time.time >= nextManaAuditTime)
        {
            LogManaAudit();
            nextManaAuditTime += 60f;
        }
    }

    void LogManaAudit()
    {
        Resource[] resources = FindObjectsByType<Resource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float sum = 0f;
        float grassMana = 0f;
        float herbivoreMana = 0f;
        float predatorMana = 0f;

        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i] == null) continue;
            float m = resources[i].mana;
            sum += m;
            switch (resources[i].resourceCategory)
            {
                case category.grass:
                    grassMana += m;
                    break;
                case category.herbivore:
                    herbivoreMana += m;
                    break;
                case category.predator:
                    predatorMana += m;
                    break;
                case category.highpredator:
                case category.dominant:
                    predatorMana += m;
                    break;
            }
        }

        float systemTotal = sum + manaPool;
        float observedMinusConfigured = systemTotal - totalMana;
        string msg = $"[ManaAudit] t={Time.time:F1}s totalResourceMana={sum:F3} poolMana={manaPool:F3} systemTotal={systemTotal:F3} observedMinusConfigured={observedMinusConfigured:F3} resourceCount={resources.Length} grass={grassMana:F3} herbivore={herbivoreMana:F3} predator={predatorMana:F3} configuredTotalMana={totalMana:F3}";
        Debug.Log(msg);
    }

    void ResetManaBudget()
    {
        manaPool = Mathf.Max(0f, totalMana);
    }

    public void ResetGenerationManaState()
    {
        manaPool = 0f;
        totalMana = 0f;
    }

    public void FinalizeGenerationManaBudget()
    {
        totalMana = Mathf.Max(0f, totalMana);
    }

    public void ConfigureManaBudget(int grassCount, int herbivoreCount, int predatorCount)
    {
        ConfigureManaBudget(grassCount, herbivoreCount, predatorCount, 0, 0);
    }

    public void ConfigureManaBudget(int grassCount, int herbivoreCount, int predatorCount, int highPredatorCount, int dominantCount)
    {
        totalMana =
            Mathf.Max(0, grassCount) * Mathf.Max(0f, manaPerGrass) +
            Mathf.Max(0, herbivoreCount) * Mathf.Max(0f, manaPerHerbivore) +
            Mathf.Max(0, predatorCount + highPredatorCount + dominantCount) * Mathf.Max(0f, manaPerPredator);
        manaPool = 0f;
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
                ResouseInit(herbivore, manaPerHerbivore, category.herbivore);
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
                ResouseInit(predator, manaPerPredator, category.predator);
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
        ResetGenerationManaState();
        HeatFieldManager.GetOrCreate().ClearAllHeat();
        ManaFieldManager.GetOrCreate().ClearAllMana();
        ConfigureManaBudget(
            grassCountPerGeneration,
            herbivoreCountPerGeneration,
            predatorCountPerGeneration,
            highPredatorCountPerGeneration,
            dominantCountPerGeneration);
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
            {
                if (grass.TryGetComponent<Resource>(out var resource))
                    resource.MarkGenerationResetDisposal();
                Destroy(grass);
            }
        }

        grasses.Clear();
        grassIndex = 0;
        pendingGrassRespawns = 0;
        nextGrassRespawnTime = 0f;
    }

    public float ConsumeGrass(GameObject grass, Resource eater)
    {
        if (grass == null || eater == null)
            return 0f;
        if (!grass.TryGetComponent<Resource>(out var grassResource))
            return 0f;
        if (grassResource.resourceCategory != category.grass)
            return 0f;

        float gained = eater.Eating(Mathf.Max(0f, grassEatAmountPerTouch), grassResource, "grass touch");
        grasses.Remove(grass);

        if (grassResource.mana > 0f)
            grassResource.RemoveMana(grassResource.mana, "grass disappear");

        Destroy(grass);
        QueueGrassRespawn();
        return gained;
    }

    void QueueGrassRespawn()
    {
        pendingGrassRespawns++;
        if (nextGrassRespawnTime <= 0f)
            nextGrassRespawnTime = Time.time + Mathf.Max(0.1f, grassSpawnCooldown);
    }

    void ProcessGrassRespawns()
    {
        if (pendingGrassRespawns <= 0 || Time.time < nextGrassRespawnTime)
            return;

        if (Addgrass())
        {
            AddExternalMana(manaPerGrass);
            pendingGrassRespawns--;
        }

        nextGrassRespawnTime = pendingGrassRespawns > 0
            ? Time.time + Mathf.Max(0.1f, grassSpawnCooldown)
            : 0f;
    }

    void PruneGrassList()
    {
        if (grasses == null)
            grasses = new List<GameObject>();

        for (int i = grasses.Count - 1; i >= 0; i--)
        {
            if (grasses[i] == null)
                grasses.RemoveAt(i);
        }
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

