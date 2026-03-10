using System.Collections.Generic;
using UnityEngine;

public class predatorManager : MonoBehaviour
{
    public List<GameObject> predators = new();
    public List<GenomePhaseBucket> genomes = new();
    public PredatorGenome genome;
    public PredatorGenome nextGenerationGenome;

    [Header("Spawn")]
    public bool useManagerGenome = false;
    public bool useNextGenerationGenome = false;
    public GameObject prefub;

    public GameObject herbivoreManager;
    public GameObject highpredatorManager;

    public bool returnHerbivores(out List<GameObject> result)
    {
        if (herbivoreManager != null && herbivoreManager.TryGetComponent<herbivoreManager>(out var hm))
        {
            result = hm.herbivores;
            return true;
        }

        result = null;
        return false;
    }

    public bool returnThreats(out List<GameObject> result)
    {
        if (highpredatorManager != null && highpredatorManager.TryGetComponent<predatorManager>(out var pm))
        {
            result = pm.predators;
            return true;
        }

        result = null;
        return false;
    }

    public bool spownpredator(GameObject Worldgen, int index, out GameObject predator)
    {
        predator = null;

        WorldGenerator wg = Worldgen.GetComponent<WorldGenerator>();
        TerrainData data = wg.terrain.terrainData;
        var rng = new System.Random(wg.seed + index + 20000);
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

            predator = Instantiate(prefub, spawnPos, Quaternion.identity);
            predator.name = $"Predator_{index}";
            predator.layer = LayerMask.NameToLayer("Creature");

            var col = new Color(
                rng.Next(0, 256) / 255f,
                rng.Next(0, 256) / 255f,
                rng.Next(0, 256) / 255f);
            var rend = predator.GetComponentInChildren<Renderer>();
            if (rend != null) rend.material.color = col;

            var pb = predator.GetComponent<predatorBehaviour>();
            if (pb == null) pb = predator.AddComponent<predatorBehaviour>();

            pb.predatorManager = this;
            pb.terrain = wg.terrain;

            PredatorGenome sourceGenome = useNextGenerationGenome ? nextGenerationGenome : genome;
            PredatorGenome g = useManagerGenome ? sourceGenome : default;
            g = ValidateOrRandomize(g, rng);
            pb.genome = g;

            predators.Add(predator);
            return true;
        }

        return false;
    }

    public void Unregister(GameObject predator)
    {
        if (predator == null) return;
        predators.Remove(predator);
    }

    static PredatorGenome ValidateOrRandomize(PredatorGenome g, System.Random rand)
    {
        bool invalid =
            g.forwardForce <= 0f ||
            g.turnForce <= 0f ||
            g.visionAngle <= 0f ||
            g.visionDistance <= 0f ||
            g.preyDetectDistance <= 0f ||
            g.visionWaves == null || g.visionWaves.Length == 0;

        if (!invalid) return g;

        g.forwardForce = 6f + (float)rand.NextDouble() * 6f;
        g.turnForce = 120f + (float)rand.NextDouble() * 320f;
        g.visionAngle = 15f + (float)rand.NextDouble() * 20f;
        g.visionTurnAngle = 5f + (float)rand.NextDouble() * 20f;
        g.visionDistance = 45f + (float)rand.NextDouble() * 65f;
        g.metabolismRate = 0.3f + (float)rand.NextDouble() * 0.9f;
        g.eatspeed = 50f + (float)rand.NextDouble() * 500f;
        g.chaseWeight = 0.8f + (float)rand.NextDouble() * 1.2f;
        g.preyDetectDistance = 20f + (float)rand.NextDouble() * 60f;
        g.attackDistance = 1.5f + (float)rand.NextDouble() * 1.5f;
        g.attackDamage = 4f + (float)rand.NextDouble() * 8f;
        g.attackCooldown = 0.4f + (float)rand.NextDouble() * 1.2f;
        g.threatWeight = 0.5f + (float)rand.NextDouble();
        g.threatDetectDistance = 15f + (float)rand.NextDouble() * 50f;
        g.memorytime = 2f + (float)rand.NextDouble() * 6f;
        g.preferredChaseDistance = 0.8f + (float)rand.NextDouble() * 3f;
        g.disengageDistance = 60f + (float)rand.NextDouble() * 70f;

        int visionWaveCount = rand.Next(1, 4);
        g.visionWaves = new WaveGene[visionWaveCount];
        for (int i = 0; i < visionWaveCount; i++)
        {
            g.visionWaves[i] = new WaveGene
            {
                frequency = 0.2f + (float)rand.NextDouble() * 5f,
                amplitude = 0.1f + (float)rand.NextDouble() * 0.9f,
                phase = (float)rand.NextDouble() * Mathf.PI * 2f
            };
        }

        int wanderWaveCount = rand.Next(1, 4);
        g.wanderWaves = new WaveGene[wanderWaveCount];
        for (int i = 0; i < wanderWaveCount; i++)
        {
            g.wanderWaves[i] = new WaveGene
            {
                frequency = 0.1f + (float)rand.NextDouble() * 3f,
                amplitude = 0.1f + (float)rand.NextDouble() * 0.8f,
                phase = (float)rand.NextDouble() * Mathf.PI * 2f
            };
        }

        return g;
    }
}
