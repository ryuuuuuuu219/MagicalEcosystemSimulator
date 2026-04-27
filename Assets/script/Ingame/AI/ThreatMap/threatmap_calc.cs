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

    [Header("Threat Score Grid")]
    public float scoreCellSize = 2f;
    public float scoreDecayPerSecond = 0.75f;
    public float scoreMinClamp = 0f;
    public float scoreMaxClamp = 1000f;
    public float lowThreatProbeDistance = 3f;
    [Range(4, 32)] public int lowThreatDirectionSamples = 12;

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
    readonly Dictionary<Vector2Int, float> threatScoreMap = new();
    readonly List<Vector2Int> decayRemoveKeys = new();

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

    public void AddThreatScore(Vector2 worldPosition, float score)
    {
        if (score == 0f) return;
        Vector2Int key = ToCell(worldPosition);
        threatScoreMap.TryGetValue(key, out float current);
        threatScoreMap[key] = Mathf.Clamp(current + score, scoreMinClamp, scoreMaxClamp);
    }

    public void RemoveThreatScore(Vector2 worldPosition, float score)
    {
        if (score == 0f) return;
        AddThreatScore(worldPosition, -Mathf.Abs(score));
    }

    public void AddThreatPulse(Vector2 worldPosition, float radius, float peakScore, int radialSamples = 12)
    {
        if (peakScore == 0f || radius <= 0f)
            return;

        AddThreatScore(worldPosition, peakScore);

        int samples = Mathf.Max(4, radialSamples);
        float safeRadius = Mathf.Max(scoreCellSize, radius);
        for (int i = 0; i < samples; i++)
        {
            float angle = (Mathf.PI * 2f * i) / samples;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            for (float dist = scoreCellSize; dist <= safeRadius; dist += scoreCellSize)
            {
                float falloff = 1f - Mathf.Clamp01(dist / safeRadius);
                if (falloff <= 0f)
                    continue;

                AddThreatScore(worldPosition + dir * dist, peakScore * falloff);
            }
        }
    }

    public Vector2 GetLowThreatDirection(Vector2 worldPosition)
    {
        int samples = Mathf.Max(4, lowThreatDirectionSamples);
        float probeDistance = Mathf.Max(0.2f, lowThreatProbeDistance);
        Vector2 bestDir = Vector2.zero;
        float bestScore = float.PositiveInfinity;

        for (int i = 0; i < samples; i++)
        {
            float ang = (Mathf.PI * 2f * i) / samples;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            Vector2 probe = worldPosition + dir * probeDistance;
            float score = GetThreatScore(probe);
            if (score < bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        return bestDir.sqrMagnitude > 0.0001f ? bestDir.normalized : Vector2.zero;
    }

    public float GetThreatScore(Vector2 worldPosition)
    {
        Vector2Int key = ToCell(worldPosition);
        return threatScoreMap.TryGetValue(key, out float value) ? value : 0f;
    }

    protected virtual void Update()
    {
        TickThreatScoreDecay(Time.deltaTime);
    }

    protected void TickThreatScoreDecay(float deltaTime)
    {
        if (threatScoreMap.Count == 0) return;

        float decay = Mathf.Max(0f, scoreDecayPerSecond) * deltaTime;
        if (decay <= 0f) return;

        decayRemoveKeys.Clear();
        var keys = ListPool<Vector2Int>.Get();
        keys.AddRange(threatScoreMap.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            Vector2Int key = keys[i];
            float v = Mathf.Max(0f, threatScoreMap[key] - decay);
            if (v <= 0.0001f)
                decayRemoveKeys.Add(key);
            else
                threatScoreMap[key] = v;
        }

        ListPool<Vector2Int>.Release(keys);

        for (int i = 0; i < decayRemoveKeys.Count; i++)
            threatScoreMap.Remove(decayRemoveKeys[i]);
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
                float dynamicThreat = GetThreatScore(new Vector2(cellPos.x, cellPos.z));
                float score = escapeMode
                    ? -(predatorField + dynamicThreat)
                    : (profile.foodWeight * foodField) +
                      (profile.corpseWeight * corpseField) -
                      (profile.predatorWeight * predatorField) -
                      dynamicThreat -
                      (settings.bowlMapWeight * bowlField);

                predatorFieldBuffer[index] = predatorField + dynamicThreat;
                cellPositionBuffer[index] = cellPos;

                if ((x != 0 || z != 0) && score > bestScore)
                {
                    bestScore = score;
                    bestIndex = index;
                }
                index++;
            }
        }

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

    Vector2Int ToCell(Vector2 worldPos)
    {
        float safe = Mathf.Max(0.1f, scoreCellSize);
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / safe),
            Mathf.FloorToInt(worldPos.y / safe));
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

static class ListPool<T>
{
    static readonly Stack<List<T>> Pool = new();

    public static List<T> Get()
    {
        return Pool.Count > 0 ? Pool.Pop() : new List<T>(64);
    }

    public static void Release(List<T> list)
    {
        list.Clear();
        Pool.Push(list);
    }
}
