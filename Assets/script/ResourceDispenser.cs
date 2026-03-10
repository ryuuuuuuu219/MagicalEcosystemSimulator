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
    public float idleEnergyCostPerSec = 0.05f;
    public float moveEnergyCostPerSec = 0.2f;

    [Header("Initial Spawn")]
    public int initialGrassCount = 100;
    public int initialHerbivoreCount = 30;
    public int initialPredatorCount = 10;   

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
        ResetCarbonBudget();
    }

    void Initialspown()
    {
        for (int i = 0; i < initialGrassCount; i++)
        {
            Addgrass();
        }

        var hm = herbivoreManager.GetComponent<herbivoreManager>();
        for (int i = 0; i < initialHerbivoreCount; i++)
        {
            if (hm.spownherbivore(Worldgen, i, out GameObject herbivore))
            {
                ResouseInit(herbivore, carbonPerHerbivore, category.herbivore);
            }
        }

        var pm = predatorManager.GetComponent<predatorManager>();
        for (int i = 0; i < initialPredatorCount; i++)
        {
            if (pm.spownpredator(Worldgen, i, out GameObject predator))
            {
                ResouseInit(predator, carbonPerPredator, category.predator);
            }
        }
    }

    void ResouseInit(GameObject obj, float amount, category category)
    {
        if (obj == null) return;

        var comp = obj.GetComponent<Resource>();
        if (comp == null)
            comp = obj.AddComponent<Resource>();

        float allocated = TakeCarbonFromPool(amount);
        comp.InitCarbon(allocated, amount);
        comp.resourceCategory = category;
    }

    public void InitializeCreatureResource(GameObject obj, float amount, category category)
    {
        if (obj == null) return;

        var comp = obj.GetComponent<Resource>();
        if (comp == null)
            comp = obj.AddComponent<Resource>();

        float allocated = TakeCarbonFromPool(amount);
        comp.InitCarbon(allocated, amount);
        comp.resourceCategory = category;
    }

    public void ReturnCarbon(float amount)
    {
        if (amount <= 0f) return;
        carbonPool += amount;
        if (totalCarbon > 0f)
            carbonPool = Mathf.Min(carbonPool, totalCarbon);
    }

    public void Addgrass()
    {
        grassIndex++;
        if (spowngrass(grassIndex, out GameObject grass))
        {
            grasses.Add(grass);
            ResouseInit(grass, carbonPerGrass, category.grass);
        }
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
        float drift = systemTotal - totalCarbon;
        string level = Mathf.Abs(drift) > 0.01f ? "Error" : "Log";
        string msg = $"[CarbonAudit] t={Time.time:F1}s totalResourceCarbon={sum:F3} poolCarbon={carbonPool:F3} systemTotal={systemTotal:F3} drift={drift:F3} resourceCount={resources.Length} grass={grassCarbon:F3} herbivore={herbivoreCarbon:F3} predator={predatorCarbon:F3} configuredTotalCarbon={totalCarbon:F3}";
        if (level == "Error")
            Debug.LogWarning(msg);
        else
            Debug.Log(msg);
    }

    void ResetCarbonBudget()
    {
        carbonPool = Mathf.Max(0f, totalCarbon);
    }

    float TakeCarbonFromPool(float requested)
    {
        float amount = Mathf.Max(0f, requested);
        if (amount <= 0f) return 0f;

        float allocated = Mathf.Min(amount, carbonPool);
        carbonPool -= allocated;
        return allocated;
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

