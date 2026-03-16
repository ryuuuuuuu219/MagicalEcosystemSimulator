using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WaveGene
{
    public float frequency;
    public float amplitude;
    public float phase;
}

[System.Serializable]
public struct HerbivoreGenome
{
    public float forwardForce;
    public float turnForce;
    public float visionAngle;
    public float visionturnAngle;
    public float visionDistance;
    public float metabolismRate;
    public float eatspeed;
    public float threatWeight;
    public float threatDetectDistance;
    public float memorytime;
    public float runAwayDistance;
    public float contactEscapeDistance;
    public float evasionAngle;
    public float evasionDuration;
    public float evasionCooldown;
    public float evasionDistance;
    public bool predictIntercept;
    public float zigzagFrequency;
    public float zigzagAmplitude;
    public float foodWeight;
    public float predatorWeight;
    public float corpseWeight;
    public float fearThreshold;
    public float escapeThreshold;
    public float curiosity;

    public WaveGene[] visionWaves;
    public WaveGene[] wanderWaves;
}

public class herbivoreBehaviour : MonoBehaviour
{
    public herbivoreManager herbivoreManager;
    public HerbivoreGenome genome;
    public Terrain terrain;

    public Resource bodyResource;
    ResourceDispenser resourceDispenser;

    [Header("Life")]
    public float maxHealth = 25f;
    public float health = 25f;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float energy = 100f;

    [Header("Sensing")]
    public float eatDistance = 1.2f;


    List<Vector2> memorygrass = new();
    List<(GameObject obj, float time)> memorythreat = new();
    public Vector2? currentTarget;

    List<GameObject> predatorObjs = new();
    List<GameObject> foodObjs = new();
    List<GameObject> corpseObjs = new();
    AnimalAICommon.MovementTelemetry lastMovementTelemetry;
    Vector3 pendingMoveVector;
    float evasionTimer = 0f;
    float evasionCooldownTimer = 0f;
    Vector3 evasionDirection;
    Vector3 currentVelocity;
    bool isEvading = false;

    public bool IsDead => health <= 0f;
    public Vector3 CurrentVelocity => currentVelocity;

    void Start()
    {
        AnimalAICommon.PrepareLegacyRigidbody(gameObject);
        bodyResource = GetComponent<Resource>();
        resourceDispenser = ResourceDispenser.Instance != null ? ResourceDispenser.Instance : FindFirstObjectByType<ResourceDispenser>();
        var gauge = GetComponent<CreatureVirtualGauge>();
        if (gauge == null)
            gauge = gameObject.AddComponent<CreatureVirtualGauge>();
        gauge.Initialize(this);
        maxHealth = CreatureBalanceTuning.HerbivoreMaxHealth;
        health = maxHealth;
        energy = maxEnergy;
        ClampEnergy();
    }

    void OnDestroy()
    {
        if (herbivoreManager != null)
            herbivoreManager.Unregister(gameObject);
    }

    void Update()
    {
        evasionTimer -= Time.deltaTime;
        evasionCooldownTimer -= Time.deltaTime;
        if (isEvading && evasionTimer <= 0f)
        {
            isEvading = false;
            evasionCooldownTimer = Mathf.Max(0f, genome.evasionCooldown);
        }

        if (bodyResource == null) return;

        if (IsDead)
        {
            pendingMoveVector = Vector3.zero;
            bodyResource.Decompose(GetDecomposeRate(), resourceDispenser);
            return;
        }

        ConvertBodyCarbonToEnergy();
        UpdateVision();
        UpdateWorldCaches();

        pendingMoveVector = ComputeTotalVector();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        health -= amount;
        if (health <= 0f)
        {
            health = 0f;
            currentTarget = null;
            pendingMoveVector = Vector3.zero;
            currentVelocity = Vector3.zero;
        }
    }

    void ConvertBodyCarbonToEnergy()
    {
        if (bodyResource == null) return;

        float carbonUsed = Mathf.Min(bodyResource.carbon, GetCarbonToEnergyRate() * Time.deltaTime);
        if (carbonUsed <= 0f) return;

        float energyGain = carbonUsed * GetMetabolicEnergyPerCarbon();
        if (WouldExceedEnergyCap(energyGain))
            return;

        float released = bodyResource.ReleaseCarbonToEnvironment(carbonUsed, resourceDispenser);
        if (released <= 0f) return;

        energy += released * GetMetabolicEnergyPerCarbon();
        ClampEnergy();

        HeatFieldManager heatField = HeatFieldManager.GetOrCreate();
        float heatAmount = released * GetMetabolicHeatPerCarbon();
        heatField.AddHeat(transform.position, heatAmount, 2f);
    }

    void ConsumeEnergy(AnimalAICommon.MovementTelemetry telemetry)
    {
        float moveCost =
            GetIdleEnergyCostPerSec() +
            GetMoveEnergyCostPerSec() * telemetry.moveDemand;
        float accelCost = GetAccelerationEnergyCostPerUnit() * telemetry.accelerationDemand;
        float brakeCost = GetBrakingEnergyCostPerUnit() * telemetry.brakingDemand;
        float turnCost = GetTurnEnergyCostPerDegree() * telemetry.turnDemand;
        float cost = moveCost * Time.fixedDeltaTime + accelCost + brakeCost + turnCost;
        energy -= cost;
        ClampEnergy();
    }

    float GetDecomposeRate()
    {
        return resourceDispenser != null ? resourceDispenser.decomposeRate : 2f;
    }

    float GetCarbonToEnergyRate()
    {
        return resourceDispenser != null ? resourceDispenser.carbonToEnergyRate : 0.5f;
    }

    float GetIdleEnergyCostPerSec()
    {
        return resourceDispenser != null ? resourceDispenser.idleEnergyCostPerSec : 0.05f;
    }

    float GetMoveEnergyCostPerSec()
    {
        return resourceDispenser != null ? resourceDispenser.moveEnergyCostPerSec : 0.2f;
    }

    float GetMetabolicEnergyPerCarbon()
    {
        return resourceDispenser != null ? resourceDispenser.metabolicEnergyPerCarbon : 1f;
    }

    float GetMetabolicHeatPerCarbon()
    {
        return resourceDispenser != null ? resourceDispenser.metabolicHeatPerCarbon : 0.5f;
    }

    float GetAccelerationEnergyCostPerUnit()
    {
        return resourceDispenser != null ? resourceDispenser.accelerationEnergyCostPerUnit : 0.03f;
    }

    float GetBrakingEnergyCostPerUnit()
    {
        return resourceDispenser != null ? resourceDispenser.brakingEnergyCostPerUnit : 0.02f;
    }

    float GetTurnEnergyCostPerDegree()
    {
        return resourceDispenser != null ? resourceDispenser.turnEnergyCostPerDegree : 0.0005f;
    }

    void ClampEnergy()
    {
        if (maxEnergy > 0f)
        {
            energy = Mathf.Clamp(energy, 0f, maxEnergy);
            return;
        }

        energy = Mathf.Max(0f, energy);
    }

    bool WouldExceedEnergyCap(float gain)
    {
        if (gain <= 0f || maxEnergy <= 0f)
            return false;

        return energy + gain > maxEnergy;
    }

    void UpdateVision()
    {
        bool gotGrasslands = herbivoreManager.returngrasses(out List<GameObject> grasslands);
        if (!gotGrasslands) return;

        Vector3 eye = transform.position + Vector3.up * 0.6f;
        Vector3 fwd = GetVisionForward();


        float cosThreshold = Mathf.Cos(genome.visionAngle * Mathf.Deg2Rad);

        GameObject best = null;
        float bestScore = 0f;

        foreach (var g in grasslands)
        {
            if (g == null) continue;

            Vector3 to = g.transform.position - eye;
            float dist = to.magnitude;
            if (dist > genome.visionDistance) continue;

            Vector3 dir = to / dist;
            float dot = Vector3.Dot(fwd, dir);
            if (dot < cosThreshold) continue;

            if (!IsGrassSafe(g.transform.position, predatorObjs))
                continue;

            if (Physics.Raycast(eye, dir, out RaycastHit hit, dist))
            {
                if (hit.collider.GetComponent<TerrainCollider>() != null)
                    continue;
            }

            float score = 1f / (dist + 1f);
            if (score > bestScore)
            {
                bestScore = score;
                best = g;
            }
        }

        if (best != null)
        {
            Vector2 p = new Vector2(best.transform.position.x, best.transform.position.z);
            if (!memorygrass.Contains(p)) memorygrass.Add(p);
            currentTarget = p;

            Debug.DrawLine(transform.position, best.transform.position, Color.magenta);
        }

        foreach (var g in memorygrass)
        {
            Vector3 p = new Vector3(g.x, transform.position.y, g.y);
            Debug.DrawLine(transform.position, p, Color.yellow);
        }
    }

    void BuildPredatorObjCache()
    {
        AnimalAICommon.BuildObjectCache(memorythreat, predatorObjs);
    }

    void BuildFoodObjCache()
    {
        foodObjs.Clear();
        if (herbivoreManager == null) return;
        if (!herbivoreManager.returngrasses(out List<GameObject> grasses) || grasses == null) return;

        for (int i = 0; i < grasses.Count; i++)
        {
            if (grasses[i] != null)
                foodObjs.Add(grasses[i]);
        }
    }

    void BuildCorpseObjCache()
    {
        corpseObjs.Clear();
        if (herbivoreManager == null) return;

        List<GameObject> herbivores = herbivoreManager.herbivores;
        for (int i = 0; i < herbivores.Count; i++)
        {
            GameObject obj = herbivores[i];
            if (obj == null) continue;
            if (!obj.TryGetComponent<herbivoreBehaviour>(out var hb)) continue;
            if (hb.IsDead)
                corpseObjs.Add(obj);
        }

        if (!herbivoreManager.returnPredators(out List<GameObject> predators) || predators == null)
            return;

        for (int i = 0; i < predators.Count; i++)
        {
            GameObject obj = predators[i];
            if (obj == null) continue;
            if (!obj.TryGetComponent<predatorBehaviour>(out var pb)) continue;
            if (pb.IsDead)
                corpseObjs.Add(obj);
        }
    }

    // 陦悟虚隧穂ｾ｡縺ｫ菴ｿ縺・黒鬟溯・・鬢後・豁ｻ鬪ｸ繧ｭ繝｣繝・す繝･繧呈峩譁ｰ縺吶ｋ縲・   
    void UpdateWorldCaches()
    {
        Detectionthreat();
        BuildPredatorObjCache();
        BuildFoodObjCache();
        BuildCorpseObjCache();
    }

    bool IsGrassSafe(Vector3 grassPos, List<GameObject> predators)
    {
        Vector3 self = transform.position;
        float R = genome.runAwayDistance;

        Vector3 toGrass = grassPos - self;
        toGrass.y = 0f;
        if (toGrass.sqrMagnitude < 0.0001f) return true;

        Vector3 grassDir = toGrass.normalized;

        foreach (var p in predators)
        {
            if (p == null) continue;

            Vector3 toPred = p.transform.position - self;
            toPred.y = 0f;
            float d = toPred.magnitude;

            if (d <= 0.001f) continue;
            if (d <= R) return false;

            float ratio = Mathf.Clamp(R / d, 0f, 1f);
            float theta = Mathf.Asin(ratio);

            Vector3 predDir = toPred / d;
            float dot = Mathf.Clamp(Vector3.Dot(predDir, grassDir), -1f, 1f);
            float phi = Mathf.Acos(dot);

            if (phi <= theta) return false;
        }
        return true;
    }

    Vector3 GetVisionForward()
    {
        float t = Time.time;
        float totalAngle = AnimalAICommon.SumWaveValue(genome.visionWaves, t);

        totalAngle = Mathf.Clamp(totalAngle, -1f, 1f);
        totalAngle *= genome.visionturnAngle;

        float halfFov = genome.visionAngle;
        float distance = genome.visionDistance;
        Vector3 origin = transform.position + Vector3.up * 0.6f;

        Vector3 left = origin + Quaternion.Euler(0f, totalAngle - halfFov, 0f) * transform.forward * distance;
        Vector3 right = origin + Quaternion.Euler(0f, totalAngle + halfFov, 0f) * transform.forward * distance;
        Debug.DrawLine(origin, left, Color.cyan);
        Debug.DrawLine(origin, right, Color.cyan);

        return Quaternion.Euler(0f, totalAngle, 0f) * transform.forward;
    }
    // 迴ｾ蝨ｨ迥ｶ諷九°繧画怙邨らｧｻ蜍輔・繧ｯ繝医Ν繧呈ｱｺ螳壹☆繧九・   
    Vector3 ComputeTotalVector()
    {
        Vector3 vEvasion = ComputeEvasionVector(out float evasionW);
        if (evasionW > 0f)
        {
            Debug.DrawLine(transform.position, transform.position + vEvasion * 3f, Color.white);
            return vEvasion;
        }

        Vector3 vFood = ComputeFoodVector(out float foodWeight, out bool isFood);
        if (isFood)
            return Vector3.zero;

        Vector3 vThreat = ComputeThreatVector(out float threatWeight);
        Vector3 vBoundary = ComputeBoundaryVector();
        Vector3 wanderBasis = vFood + vBoundary;
        if (wanderBasis.sqrMagnitude < 0.0001f)
            wanderBasis = transform.forward;
        Vector3 vWander = ComputeWanderVector(wanderBasis);

        float wFood = Mathf.Clamp01(foodWeight) * genome.foodWeight;
        float wThreat = Mathf.Clamp(threatWeight, 0f, 2f) * genome.predatorWeight;
        float wBoundary = 1f;
        float wWander = Mathf.Clamp01(1f - Mathf.Clamp01(wFood) - Mathf.Clamp01(wThreat));

        if (wThreat > Mathf.Max(0.1f, genome.escapeThreshold))
        {
            wFood *= 0.25f;
            wWander *= 0.25f;
        }

        Vector3 total =
            wFood * vFood +
            wThreat * vThreat +
            wBoundary * vBoundary +
            wWander * vWander;
        total = AnimalAICommon.AdjustMovementVectorForTerrain(terrain, transform.position, total);

        if (total.sqrMagnitude <= 0.0001f)
            total = AnimalAICommon.AdjustMovementVectorForTerrain(
                terrain,
                transform.position,
                ComputeWanderVector(transform.forward) * Mathf.Max(0f, genome.curiosity));

        Debug.DrawLine(transform.position, transform.position + vFood, Color.green);
        Debug.DrawLine(transform.position, transform.position + vThreat, Color.red);
        Debug.DrawLine(transform.position, transform.position + vBoundary, Color.blue);
        Debug.DrawLine(transform.position, transform.position + vWander, Color.yellow);

        return total.normalized;
    }

    // 霑大ｍ縺ｫ鬢後′縺ゅｋ蝣ｴ蜷医↓鞫る｣溘＠縲∵・蜉溷庄蜷ｦ繧定ｿ斐☆縲・   
    bool TryEatNearby()
    {
        Vector3 self = transform.position;
        float bestDist = float.MaxValue;
        Vector3 target = self;
        bool found = false;

        for (int i = 0; i < foodObjs.Count; i++)
        {
            GameObject grass = foodObjs[i];
            if (grass == null) continue;

            Vector3 gp = grass.transform.position;
            float dist = Vector3.Distance(self, gp);
            if (dist > eatDistance || dist >= bestDist) continue;

            bestDist = dist;
            target = gp;
            found = true;
        }

        if (!found)
            return false;

        Eat(target);
        return true;
    }

    bool IsThreatCloseWhileEating()
    {
        float trigger = Mathf.Max(0.1f, genome.contactEscapeDistance);
        Vector3 self = transform.position;

        for (int i = 0; i < predatorObjs.Count; i++)
        {
            GameObject pred = predatorObjs[i];
            if (pred == null) continue;

            float dist = Vector3.Distance(self, pred.transform.position);
            if (dist <= trigger) return true;
        }

        return false;
    }

    Vector3 ComputeEvasionVector(out float evasionweight)
    {
        if (isEvading && evasionTimer > 0f)
        {
            Vector3 dir = evasionDirection;

            if (genome.zigzagAmplitude > 0f && genome.zigzagFrequency > 0f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                float zigzag = Mathf.Sin(Time.time * genome.zigzagFrequency) * genome.zigzagAmplitude;
                dir = (dir + side * zigzag).normalized;
            }

            evasionweight = 1.5f;
            return dir;
        }

        if (evasionCooldownTimer > 0f)
        {
            evasionweight = 0f;
            return Vector3.zero;
        }

        float triggerDistance = Mathf.Max(0.1f, genome.evasionDistance);
        GameObject closestPredator = null;
        float closestDist = float.MaxValue;
        Vector3 self = transform.position;

        for (int i = 0; i < predatorObjs.Count; i++)
        {
            GameObject pred = predatorObjs[i];
            if (pred == null) continue;

            float dist = Vector3.Distance(self, pred.transform.position);
            if (dist > triggerDistance) continue;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestPredator = pred;
            }
        }

        if (closestPredator == null)
        {
            evasionweight = 0f;
            return Vector3.zero;
        }

        Vector3 away = self - closestPredator.transform.position;
        away.y = 0f;
        if (away.sqrMagnitude <= 0.0001f)
        {
            evasionweight = 0f;
            return Vector3.zero;
        }

        away.Normalize();
        float signedAngle = Random.value < 0.5f ? -genome.evasionAngle : genome.evasionAngle;
        evasionDirection = Quaternion.Euler(0f, signedAngle, 0f) * away;
        evasionDirection.Normalize();

        isEvading = true;
        evasionTimer = Mathf.Max(0.05f, genome.evasionDuration);

        Vector3 startDir = evasionDirection;
        if (genome.zigzagAmplitude > 0f && genome.zigzagFrequency > 0f)
        {
            Vector3 side = Vector3.Cross(Vector3.up, startDir).normalized;
            float zigzag = Mathf.Sin(Time.time * genome.zigzagFrequency) * genome.zigzagAmplitude;
            startDir = (startDir + side * zigzag).normalized;
        }

        evasionweight = 1.5f;
        return startDir;
    }

    Vector3 ComputeThreatVector(out float threatweight)
    {
        Vector3 pos = transform.position;
        Vector3 away = Vector3.zero;
        threatweight = 0f;

        for (int i = 0; i < predatorObjs.Count; i++)
        {
            GameObject p = predatorObjs[i];
            if (p == null) continue;

            Vector3 toPred = p.transform.position - pos;
            toPred.y = 0f;
            float dist = toPred.magnitude;
            if (dist <= 0.001f) continue;
            float strength = ComputeThreatLevel(dist);

            away += (-toPred / dist) * strength;
            if (strength > threatweight)
            {
                threatweight = strength;
            }
        }

        away.y = 0f;
        return away;
    }

    float ComputeThreatLevel(float dist)
    {
        return AnimalAICommon.ComputeThreatLevel(dist, genome.threatDetectDistance);
    }

    void Detectionthreat()
    {
        AnimalAICommon.TickMemory(memorythreat, Time.deltaTime);

        herbivoreManager.returnPredators(out List<GameObject> preds);
        Vector3 pos = transform.position;

        foreach (var p in preds)
        {
            if (p == null) continue;

            float dist = Vector3.Distance(p.transform.position, pos);
            if (dist <= 0.001f) continue;
            if (dist > genome.threatDetectDistance) continue;

            AnimalAICommon.Remember(memorythreat, p, genome.memorytime);
        }
    }

    Vector3 ComputeFoodVector(out float foodweight, out bool isfood)
    {
        isfood = false;

        if (!currentTarget.HasValue)
        {
            if (memorygrass == null)
            {
                foodweight = 0.6f;
                return Vector3.forward;
            }

            foreach (var p in memorygrass)
            {
                Vector3 gp = new Vector3(p.x, transform.position.y, p.y);
                if (IsGrassSafe(gp, predatorObjs))
                {
                    currentTarget = p;
                    break;
                }
            }
        }

        if (!currentTarget.HasValue)
        {
            foodweight = 0f;
            return Vector3.zero;
        }

        Vector3 pos = transform.position;
        Vector3 target = new Vector3(currentTarget.Value.x, pos.y, currentTarget.Value.y);
        float dist = Vector3.Distance(pos, target);

        if (dist < eatDistance)
        {
            Eat(target);
            foodweight = 0f;
            isfood = true;
            return Vector3.zero;
        }

        foodweight = 1f - Mathf.Clamp01(dist / genome.visionDistance);
        return (target - pos).normalized;
    }

    Vector3 ComputeWanderVector(Vector3 v)
    {
        float t = Time.time;
        float totalAngle = AnimalAICommon.SumWaveValue(genome.wanderWaves, t);

        if (genome.wanderWaves == null || genome.wanderWaves.Length == 0)
        {
            totalAngle =
                Mathf.Sin(t * genome.metabolismRate) +
                Mathf.Sin(t * genome.metabolismRate * 0.5f) * 0.5f;
        }

        totalAngle = Mathf.Clamp(totalAngle, -1f, 1f);

        Vector3 dir = Quaternion.Euler(0, totalAngle * 45f, 0) * v;
        return dir.normalized;
    }

    Vector3 ComputeBoundaryVector()
    {
        return AnimalAICommon.ComputeBoundaryVector(terrain, transform.position);
    }

    float currentSpeed = 0f;

    AnimalAICommon.MovementTelemetry ApplyMovement(Vector3 total)
    {
        float movementCapacity = GetMovementCapacity();
        if (movementCapacity <= 0f)
            return AnimalAICommon.ApplyMovement(
                transform,
                terrain,
                Vector3.zero,
                ref currentSpeed,
                ref currentVelocity,
                genome.forwardForce,
                genome.turnForce,
                Time.fixedDeltaTime);

        return AnimalAICommon.ApplyMovement(
            transform,
            terrain,
            total,
            ref currentSpeed,
            ref currentVelocity,
            genome.forwardForce * movementCapacity,
            genome.turnForce,
            Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        if (bodyResource != null && !IsDead)
        {
            lastMovementTelemetry = ApplyMovement(pendingMoveVector);
            ConsumeEnergy(lastMovementTelemetry);
        }

        ClampRotation();
    }

    void ClampRotation()
    {
        AnimalAICommon.ClampRotation(transform, terrain);
    }

    void Eat(Vector3 target)
    {
        Collider[] hits = Physics.OverlapSphere(target, 1f);

        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Resource>(out var resource)) continue;
            if (resource.resourceCategory != Resource.category.grass) continue;

            bodyResource.Eating(genome.eatspeed * Time.deltaTime, resource);
        }
    }

    float GetMovementCapacity()
    {
        if (maxEnergy <= 0f)
            return 1f;

        float ratio = Mathf.Clamp01(energy / maxEnergy);
        if (ratio <= 0.05f)
            return 0f;

        return Mathf.InverseLerp(0.05f, 0.25f, ratio);
    }
}

