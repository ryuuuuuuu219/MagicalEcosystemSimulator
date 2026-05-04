using System;
using System.Collections.Generic;
using UnityEngine;

public class threatmap_calc : MonoBehaviour
{
    public struct EvaluationProfile
    {
        public float foodWeight;
        public float predatorWeight;
        public float corpseWeight;
        public float fearThreshold;
    }

    public struct Settings
    {
        public float cellSize;
        public float mapRadius;
        public float predatorFieldIntensity;
        public float predatorFieldDecay;
        public float predatorFieldRadius;
        public float foodFieldIntensity;
        public float foodFieldDecay;
        public float foodFieldRadius;
        public float corpseFieldIntensity;
        public float corpseFieldDecay;
        public float corpseFieldRadius;
        public float bowlMapWeight;
    }

    [Serializable]
    public struct AgentTypeBinding
    {
        public AiAgentMode mode;
        public string behaviourTypeName;
        public string genomeTypeName;
    }

    [Header("AI Mode")]
    public AiAgentMode currentMode = AiAgentMode.herbibore;
    public AgentTypeBinding[] modeBindings =
    {
        new AgentTypeBinding
        {
            mode = AiAgentMode.grassland,
            behaviourTypeName = "grassland",
            genomeTypeName = ""
        },
        new AgentTypeBinding
        {
            mode = AiAgentMode.herbibore,
            behaviourTypeName = "herbivoreBehaviour",
            genomeTypeName = "HerbivoreGenome"
        },
        new AgentTypeBinding
        {
            mode = AiAgentMode.predator,
            behaviourTypeName = "predatorBehaviour",
            genomeTypeName = "PredatorGenome"
        },
        new AgentTypeBinding
        {
            mode = AiAgentMode.highpredator,
            behaviourTypeName = "predatorBehaviour",
            genomeTypeName = "PredatorGenome"
        }
    };

    protected float[] predatorFieldBuffer;
    protected Vector3[] cellPositionBuffer;
    protected int evaluatedCellCount;
    protected Settings lastEvaluationSettings;

    public Type CurrentBehaviourType => GetBehaviourType(currentMode);
    public Type CurrentGenomeType => GetGenomeType(currentMode);

    public void SetMode(AiAgentMode mode)
    {
        currentMode = mode;
    }

    public Type GetBehaviourType(AiAgentMode mode)
    {
        string typeName = ResolveTypeName(mode, true);
        return ResolveType(typeName);
    }

    public Type GetGenomeType(AiAgentMode mode)
    {
        string typeName = ResolveTypeName(mode, false);
        return ResolveType(typeName);
    }

    public float SampleEvaluatedThreat(Vector3 worldPosition)
    {
        if (predatorFieldBuffer == null || cellPositionBuffer == null || evaluatedCellCount <= 0)
            return 0f;

        float bestDistanceSqr = float.PositiveInfinity;
        float bestValue = 0f;
        Vector3 flatPosition = new Vector3(worldPosition.x, 0f, worldPosition.z);
        int count = Mathf.Min(evaluatedCellCount, predatorFieldBuffer.Length);

        for (int i = 0; i < count; i++)
        {
            Vector3 cell = cellPositionBuffer[i];
            cell.y = 0f;
            float distanceSqr = (cell - flatPosition).sqrMagnitude;
            if (distanceSqr >= bestDistanceSqr)
                continue;

            bestDistanceSqr = distanceSqr;
            bestValue = predatorFieldBuffer[i];
        }

        float cellSize = Mathf.Max(0.5f, lastEvaluationSettings.cellSize);
        float maxDistance = cellSize * 1.2f;
        return bestDistanceSqr <= maxDistance * maxDistance ? bestValue : 0f;
    }

    public float ComputePredatorField(
        Vector3 position,
        List<GameObject> predators,
        in Settings settings)
    {
        return AccumulateExponentialField(
            position,
            predators,
            settings.predatorFieldIntensity,
            settings.predatorFieldDecay,
            settings.predatorFieldRadius);
    }

    public Vector3 EvaluateBestDirection(
        Vector3 basePos,
        Terrain terrain,
        HerbivoreGenome genome,
        bool escapeMode,
        List<GameObject> predators,
        List<GameObject> foods,
        List<GameObject> corpses,
        in Settings settings,
        Transform debugOwner)
    {
        EvaluationProfile profile = new EvaluationProfile
        {
            foodWeight = genome.foodWeight,
            predatorWeight = genome.predatorWeight,
            corpseWeight = genome.corpseWeight,
            fearThreshold = genome.fearThreshold
        };

        return EvaluateBestDirection(
            basePos,
            terrain,
            profile,
            escapeMode,
            predators,
            foods,
            corpses,
            settings,
            debugOwner);
    }

    public Vector3 EvaluateBestDirection(
        Vector3 basePos,
        Terrain terrain,
        EvaluationProfile profile,
        bool escapeMode,
        List<GameObject> predators,
        List<GameObject> foods,
        List<GameObject> corpses,
        in Settings settings,
        Transform debugOwner)
    {
        int half = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(1f, settings.mapRadius) / Mathf.Max(0.5f, settings.cellSize)));
        int side = half * 2 + 1;
        int total = side * side;
        EnsureBuffers(total);

        int bestIndex = -1;
        float bestScore = float.NegativeInfinity;
        int index = 0;
        float safeFearThreshold = Mathf.Max(0f, profile.fearThreshold);

        for (int z = -half; z <= half; z++)
        {
            for (int x = -half; x <= half; x++)
            {
                Vector3 cellPos = basePos + new Vector3(x * settings.cellSize, 0f, z * settings.cellSize);

                float predatorField = ComputePredatorField(cellPos, predators, settings);
                float foodField = AccumulateExponentialField(cellPos, foods, settings.foodFieldIntensity, settings.foodFieldDecay, settings.foodFieldRadius);
                float corpseField = AccumulateExponentialField(cellPos, corpses, settings.corpseFieldIntensity, settings.corpseFieldDecay, settings.corpseFieldRadius);
                if (predatorField > safeFearThreshold)
                    corpseField = 0f;

                float bowlField = ComputeCenterDistanceSquaredField(cellPos, terrain);
                float score = escapeMode
                    ? -predatorField
                    : (profile.foodWeight * foodField) +
                      (profile.corpseWeight * corpseField) -
                      (profile.predatorWeight * predatorField) -
                      (settings.bowlMapWeight * bowlField);

                predatorFieldBuffer[index] = predatorField;
                cellPositionBuffer[index] = cellPos;

                if ((x != 0 || z != 0) && score > bestScore)
                {
                    bestScore = score;
                    bestIndex = index;
                }
                index++;
            }
        }

        evaluatedCellCount = index;
        lastEvaluationSettings = settings;
        OnMapEvaluated(index, settings, debugOwner);

        if (bestIndex < 0)
            return Vector3.zero;

        Vector3 dir = cellPositionBuffer[bestIndex] - basePos;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f)
            return Vector3.zero;
        return dir.normalized;
    }

    protected virtual void OnMapEvaluated(int count, in Settings settings, Transform debugOwner)
    {
    }

    string ResolveTypeName(AiAgentMode mode, bool behaviour)
    {
        for (int i = 0; i < modeBindings.Length; i++)
        {
            if (modeBindings[i].mode != mode) continue;
            return behaviour ? modeBindings[i].behaviourTypeName : modeBindings[i].genomeTypeName;
        }
        return string.Empty;
    }

    Type ResolveType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        Type t = Type.GetType(typeName);
        if (t != null)
            return t;

        var asm = typeof(threatmap_calc).Assembly;
        return asm.GetType(typeName);
    }

    protected float AccumulateExponentialField(
        Vector3 position,
        List<GameObject> sources,
        float intensity,
        float decay,
        float radius)
    {
        if (sources == null || sources.Count == 0 || intensity <= 0f || radius <= 0f)
            return 0f;

        float safeDecay = Mathf.Max(0.01f, decay);
        float safeRadius = Mathf.Max(0.1f, radius);
        float total = 0f;
        Vector3 flatPos = new Vector3(position.x, 0f, position.z);

        for (int i = 0; i < sources.Count; i++)
        {
            GameObject src = sources[i];
            if (src == null) continue;

            Vector3 srcPos = src.transform.position;
            float dist = Vector3.Distance(flatPos, new Vector3(srcPos.x, 0f, srcPos.z));
            if (dist > safeRadius) continue;
            total += intensity * Mathf.Exp(-dist / safeDecay);
        }

        return total;
    }

    float ComputeCenterDistanceSquaredField(Vector3 position, Terrain terrain)
    {
        if (terrain == null || terrain.terrainData == null)
            return 0f;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        Vector3 center = terrainPos + new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);

        float safeHalfX = Mathf.Max(1f, size.x * 0.5f);
        float safeHalfZ = Mathf.Max(1f, size.z * 0.5f);

        float dx = (position.x - center.x) / safeHalfX;
        float dz = (position.z - center.z) / safeHalfZ;
        return dx * dx + dz * dz;
    }

    void EnsureBuffers(int requiredSize)
    {
        if (predatorFieldBuffer == null || predatorFieldBuffer.Length != requiredSize)
        {
            predatorFieldBuffer = new float[requiredSize];
            cellPositionBuffer = new Vector3[requiredSize];
        }
    }
}
