using System.Collections.Generic;
using UnityEngine;

public class herbivoreManager : MonoBehaviour
{
    public List<GameObject> herbivores = new();
    public List<GenomePhaseBucket> genomes = new();
    public HerbivoreGenome genome;
    public HerbivoreGenome nextGenerationGenome;

    [Header("Spawn")]
    public bool useManagerGenome = false;
    public bool useNextGenerationGenome = false;
    public GameObject prefub;

    public GameObject grassManager;
    public GameObject predatorManager;

    public bool returngrasses(out List<GameObject> result)
    {
        if (grassManager != null && grassManager.TryGetComponent<ResourceDispenser>(out var gm))
        {
            result = gm.grasses;
            return true;
        }
        result = null;
        return false;
    }

    public bool returnPredators(out List<GameObject> result)
    {
        if (predatorManager != null && predatorManager.TryGetComponent<predatorManager>(out var pm))
        {
            result = pm.predators;
            return true;
        }
        result = null;
        return false;
    }

    public bool spownherbivore(GameObject Worldgen, int index, out GameObject herbivore)
    {
        herbivore = null;

        WorldGenerator wg = Worldgen.GetComponent<WorldGenerator>();
        TerrainData data = wg.terrain.terrainData;

        var rng = new System.Random(wg.seed + index + 10000);
        int maxTry = 10;

        for (int t = 0; t < maxTry; t++)
        {
            float centerX = (float)rng.NextDouble() * wg.terrainSize;
            float centerZ = (float)rng.NextDouble() * wg.terrainSize;

            float normX = (centerX - wg.terrain.transform.position.x) / data.size.x;
            float normZ = (centerZ - wg.terrain.transform.position.z) / data.size.z;

            float height = data.GetInterpolatedHeight(normX, normZ);
            if (height <= wg.waterHeight) continue;

            Vector3 spawnPos = new Vector3(centerX, height, centerZ) + new Vector3(0, 2f, 0);
            Quaternion spawnRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);

            herbivore = Instantiate(prefub, spawnPos, spawnRotation);
            herbivore.name = $"Herbivore_{index}";
            herbivore.layer = LayerMask.NameToLayer("Creature");

            var col = new Color(
                rng.Next(0, 256) / 255f,
                rng.Next(0, 256) / 255f,
                rng.Next(0, 256) / 255f);

            var rend = herbivore.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = col;

            var hb = herbivore.GetComponent<herbivoreBehaviour>();
            if (hb == null) hb = herbivore.AddComponent<herbivoreBehaviour>();

            hb.herbivoreManager = this;
            hb.terrain = wg.terrain;

            HerbivoreGenome sourceGenome = useNextGenerationGenome ? nextGenerationGenome : genome;
            HerbivoreGenome g = useManagerGenome ? sourceGenome : default;
            g = ValidateOrRandomize(g, rng);
            hb.genome = g;

            herbivores.Add(herbivore);
            return true;
        }

        return false;
    }

    public bool SpawnHerbivoreWithGenome(GameObject worldgen, int index, HerbivoreGenome injectedGenome, out GameObject herbivore)
    {
        if (!spownherbivore(worldgen, index, out herbivore))
            return false;

        if (herbivore != null && herbivore.TryGetComponent<herbivoreBehaviour>(out var hb))
        {
            hb.genome = injectedGenome;
        }

        return herbivore != null;
    }

    public void Unregister(GameObject herbivore)
    {
        if (herbivore == null) return;
        herbivores.Remove(herbivore);
    }

    static HerbivoreGenome ValidateOrRandomize(HerbivoreGenome g, System.Random rand)
    {
        bool invalid =
            g.forwardForce <= 0f ||
            g.turnForce <= 0f ||
            g.visionAngle <= 0f ||
            g.visionDistance <= 0f ||
            g.foodWeight <= 0f ||
            g.predatorWeight <= 0f ||
            g.escapeThreshold <= 0f ||
            g.visionWaves == null || g.visionWaves.Length == 0;

        if (!invalid) return g;

        g.forwardForce = 5f + (float)rand.NextDouble() * 5f;
        g.turnForce = 100f + (float)rand.NextDouble() * 300f;
        g.visionAngle = 4f + (float)rand.NextDouble() * 5f;
        g.visionturnAngle = 15f + (float)rand.NextDouble() * 35f;
        g.visionDistance = 50f + (float)rand.NextDouble() * 50f;
        g.metabolismRate = (float)rand.NextDouble();
        g.eatspeed = (float)rand.NextDouble() * 500f;
        g.threatWeight = (float)rand.NextDouble();
        g.threatDetectDistance = 20f + (float)rand.NextDouble() * 60f;
        g.memorytime = 1f + (float)rand.NextDouble() * 4f;
        g.runAwayDistance = 5f + (float)rand.NextDouble() * 30f;
        g.contactEscapeDistance = 3f + (float)rand.NextDouble() * 20f;
        g.evasionAngle = 10f + (float)rand.NextDouble() * 35f;
        g.evasionDuration = 0.2f + (float)rand.NextDouble() * 1.0f;
        g.evasionCooldown = 0.3f + (float)rand.NextDouble() * 1.5f;
        g.evasionDistance = 4f + (float)rand.NextDouble() * 12f;
        g.predictIntercept = rand.NextDouble() < 0.5;
        g.zigzagFrequency = 1f + (float)rand.NextDouble() * 4f;
        g.zigzagAmplitude = (float)rand.NextDouble() * 0.7f;
        g.foodWeight = 1f + ((float)rand.NextDouble() - 0.5f) * 0.4f;
        g.predatorWeight = 3f + ((float)rand.NextDouble() - 0.5f) * 1.2f;
        g.corpseWeight = 1.2f + ((float)rand.NextDouble() - 0.5f) * 0.6f;
        g.fearThreshold = 2f + ((float)rand.NextDouble() - 0.5f) * 0.8f;
        g.escapeThreshold = 4f + ((float)rand.NextDouble() - 0.5f) * 1.2f;
        g.curiosity = (float)rand.NextDouble();

        int waveCount = rand.Next(1, 4);
        g.visionWaves = new WaveGene[waveCount];
        g.wanderWaves = new WaveGene[waveCount];
        for (int i = 0; i < waveCount; i++)
        {
            g.visionWaves[i] = new WaveGene
            {
                frequency = 0.2f + (float)rand.NextDouble() * 5f,
                amplitude = 0.1f + (float)rand.NextDouble() * 0.9f,
                phase = (float)rand.NextDouble() * Mathf.PI * 2f
            };
            g.wanderWaves[i] = new WaveGene
            {
                frequency = 0.2f + (float)rand.NextDouble() * 4f,
                amplitude = 0.05f + (float)rand.NextDouble() * 0.7f,
                phase = (float)rand.NextDouble() * Mathf.PI * 2f
            };
        }
        return g;
    }
}
