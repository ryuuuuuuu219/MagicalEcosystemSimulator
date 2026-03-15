using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PredatorGenome
{
    public float forwardForce;
    public float turnForce;
    public float visionAngle;
    public float visionTurnAngle;
    public float visionDistance;
    public float metabolismRate;
    public float eatspeed;
    public float chaseWeight;
    public float preyDetectDistance;
    public float attackDistance;
    public float attackDamage;
    public float attackCooldown;
    public float threatWeight;
    public float threatDetectDistance;
    public float memorytime;
    public float preferredChaseDistance;
    public float disengageDistance;
    public float stopMoveThreshold;
    public float resumeMoveThreshold;
    public AttackArcSettings chargeArc;
    public float chargeDamageScale;
    public float chargeEnergyCost;
    public float chargeContactPadding;
    public float chargeAttackClock;
    public AttackArcSettings biteArc;
    public float biteDamage;
    public float biteEnergyCost;
    public float biteAttackClock;
    public AttackArcSettings meleeArc;
    public float meleeDamage;
    public float meleeEnergyCost;
    public float meleeAttackClock;
    public float attackThreatPulseScore;
    public float attackThreatPulseRadius;
    public float attackTraceScale;
    public float attackTraceDuration;
    public float attackTraceDepth;

    public WaveGene[] visionWaves;
    public WaveGene[] wanderWaves;
}

public class predatorBehaviour : MonoBehaviour
{
    public predatorManager predatorManager;
    public PredatorGenome genome;
    public Terrain terrain;

    public Resource bodyResource;
    ResourceDispenser resourceDispenser;

    [Header("Life")]
    public float maxHealth = 40f;
    public float health = 40f;

    [Header("Energy")]
    public float maxEnergy = 120f;
    public float energy = 120f;

    [Header("Sensing")]
    public float eatDistance = 1.2f;

    List<(GameObject obj, float time)> memoryPrey = new();
    List<(GameObject obj, float time)> memoryThreat = new();
    List<GameObject> preyObjs = new();
    List<GameObject> threatObjs = new();

    GameObject trackedPrey;
    Vector3 lastTrackedPreyPosition;
    Vector3 trackedPreyVelocity;
    Vector3 currentVelocity;
    bool hasTrackedPreySample;
    bool isMovementSuppressed;
    AnimalAICommon.MovementTelemetry lastMovementTelemetry;
    Vector3 pendingMoveVector;
    threatmap_calc threatMap;
    PredatorCombatLibrary.CombatState combatState = new PredatorCombatLibrary.CombatState
    {
        lastChargeClockTime = -999f,
        lastBiteClockTime = -999f,
        lastMeleeClockTime = -999f
    };

    public Vector2? currentTarget;
    public bool IsDead => health <= 0f;
    public Vector3 CurrentVelocity => currentVelocity;

    void Start()
    {
        AnimalAICommon.PrepareLegacyRigidbody(gameObject);
        bodyResource = GetComponent<Resource>();
        resourceDispenser = ResourceDispenser.Instance != null ? ResourceDispenser.Instance : FindFirstObjectByType<ResourceDispenser>();
        threatMap = FindFirstObjectByType<threatmap_calc>();
        var gauge = GetComponent<CreatureVirtualGauge>();
        if (gauge == null)
            gauge = gameObject.AddComponent<CreatureVirtualGauge>();
        gauge.Initialize(this);
        health = maxHealth;
        energy = maxEnergy;
        ClampEnergy();
    }

    void OnDestroy()
    {
        if (predatorManager != null)
            predatorManager.Unregister(gameObject);
    }

    void Update()
    {
        if (bodyResource == null) return;

        if (IsDead)
        {
            pendingMoveVector = Vector3.zero;
            bodyResource.Decompose(GetDecomposeRate(), resourceDispenser);
            return;
        }

        ConvertBodyCarbonToEnergy();
        UpdateVision();

        pendingMoveVector = ComputeTotalVector();
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
        DetectionPrey();
        DetectionThreat();
        BuildCaches();

        Vector3 eye = transform.position + Vector3.up * 0.6f;
        Vector3 fwd = GetVisionForward();
        float cosThreshold = Mathf.Cos(genome.visionAngle * Mathf.Deg2Rad);

        GameObject best = null;
        float bestScore = 0f;

        for (int i = 0; i < preyObjs.Count; i++)
        {
            GameObject prey = preyObjs[i];
            if (prey == null) continue;
            if (!prey.TryGetComponent<Resource>(out var res)) continue;
            if (res.resourceCategory != Resource.category.herbivore) continue;

            Vector3 to = prey.transform.position - eye;
            float dist = to.magnitude;
            if (dist <= 0.001f || dist > genome.visionDistance || dist > genome.disengageDistance) continue;

            Vector3 dir = to / dist;
            float dot = Vector3.Dot(fwd, dir);
            if (dot < cosThreshold) continue;

            if (Physics.Raycast(eye, dir, out RaycastHit hit, dist))
            {
                if (hit.collider.GetComponent<TerrainCollider>() != null)
                    continue;
            }

            float score = 1f / (dist + 1f);
            if (score > bestScore)
            {
                bestScore = score;
                best = prey;
            }
        }

        if (best != null)
        {
            currentTarget = new Vector2(best.transform.position.x, best.transform.position.z);
            Debug.DrawLine(transform.position, best.transform.position, Color.magenta);
        }
        else if (currentTarget.HasValue)
        {
            Vector3 target = new Vector3(currentTarget.Value.x, transform.position.y, currentTarget.Value.y);
            if (Vector3.Distance(transform.position, target) > genome.disengageDistance)
                currentTarget = null;
        }
    }

    void DetectionPrey()
    {
        TickMemory(memoryPrey);

        if (!predatorManager.returnHerbivores(out List<GameObject> herbivores))
            return;

        Vector3 pos = transform.position;

        foreach (var prey in herbivores)
        {
            if (prey == null) continue;
            if (!prey.TryGetComponent<Resource>(out var res)) continue;
            if (res.resourceCategory != Resource.category.herbivore) continue;

            float dist = Vector3.Distance(prey.transform.position, pos);
            if (dist <= 0.001f || dist > genome.preyDetectDistance) continue;

            Remember(memoryPrey, prey);
        }
    }

    void DetectionThreat()
    {
        TickMemory(memoryThreat);

        if (!predatorManager.returnThreats(out List<GameObject> threats))
            return;

        Vector3 pos = transform.position;

        foreach (var threat in threats)
        {
            if (threat == null || threat == gameObject) continue;

            float dist = Vector3.Distance(threat.transform.position, pos);
            if (dist <= 0.001f || dist > genome.threatDetectDistance) continue;

            Remember(memoryThreat, threat);
        }
    }

    void TickMemory(List<(GameObject obj, float time)> memory)
    {
        AnimalAICommon.TickMemory(memory, Time.deltaTime);
    }

    void Remember(List<(GameObject obj, float time)> memory, GameObject obj)
    {
        AnimalAICommon.Remember(memory, obj, genome.memorytime);
    }

    void BuildCaches()
    {
        AnimalAICommon.BuildObjectCache(memoryPrey, preyObjs);
        AnimalAICommon.BuildObjectCache(memoryThreat, threatObjs);
    }

    Vector3 GetVisionForward()
    {
        float t = Time.time;
        float totalAngle = AnimalAICommon.SumWaveValue(genome.visionWaves, t);

        totalAngle = Mathf.Clamp(totalAngle, -1f, 1f) * genome.visionTurnAngle;

        float halfFov = genome.visionAngle;
        float distance = genome.visionDistance;
        Vector3 origin = transform.position + Vector3.up * 0.6f;

        Vector3 left = origin + Quaternion.Euler(0f, totalAngle - halfFov, 0f) * transform.forward * distance;
        Vector3 right = origin + Quaternion.Euler(0f, totalAngle + halfFov, 0f) * transform.forward * distance;
        Debug.DrawLine(origin, left, Color.cyan);
        Debug.DrawLine(origin, right, Color.cyan);

        return Quaternion.Euler(0f, totalAngle, 0f) * transform.forward;
    }

    Vector3 ComputeTotalVector()
    {
        Vector3 vPrey = ComputePreyVector(out float preyWeight);
        Vector3 vThreat = ComputeThreatVector(out float threatWeight);
        Vector3 vBoundary = ComputeBoundaryVector();
        Vector3 wanderBasis = vPrey + vBoundary;
        if (wanderBasis.sqrMagnitude < 0.0001f) wanderBasis = transform.forward;
        Vector3 vWander = ComputeWanderVector(wanderBasis);

        float wPrey = Mathf.Clamp01(preyWeight) * genome.chaseWeight;
        float wThreat = Mathf.Clamp(threatWeight, 0f, 2f) * genome.threatWeight;
        float wBoundary = 1f;
        float wWander = Mathf.Clamp01(1f - Mathf.Clamp01(wPrey) - Mathf.Clamp01(wThreat));

        if (wThreat > 0.5f)
        {
            wPrey *= 0.25f;
            wWander *= 0.25f;
        }

        Vector3 total =
            wPrey * vPrey +
            wThreat * vThreat +
            wBoundary * vBoundary +
            wWander * vWander;
        total = AnimalAICommon.AdjustMovementVectorForTerrain(terrain, transform.position, total);

        Debug.DrawLine(transform.position, transform.position + vPrey, Color.red);
        Debug.DrawLine(transform.position, transform.position + vThreat, Color.magenta);
        Debug.DrawLine(transform.position, transform.position + vBoundary, Color.blue);
        Debug.DrawLine(transform.position, transform.position + vWander, Color.green);

        float stopThreshold = Mathf.Max(0.001f, genome.stopMoveThreshold);
        float resumeThreshold = Mathf.Max(stopThreshold + 0.001f, genome.resumeMoveThreshold);
        float totalMagnitude = total.magnitude;
        if (isMovementSuppressed)
        {
            if (totalMagnitude <= resumeThreshold)
                return Vector3.zero;

            isMovementSuppressed = false;
        }
        else if (totalMagnitude <= stopThreshold)
        {
            isMovementSuppressed = true;
            return Vector3.zero;
        }

        return total;
    }

    Vector3 ComputePreyVector(out float preyWeight)
    {
        GameObject bestPrey = GetBestRememberedPrey();
        if (bestPrey == null)
        {
            currentTarget = null;
            preyWeight = 0f;
            return Vector3.zero;
        }

        Vector3 pos = transform.position;
        Vector3 target = bestPrey.transform.position;
        currentTarget = new Vector2(target.x, target.z);
        UpdateTrackedPreyMotion(bestPrey, target);

        float dist = Vector3.Distance(pos, target);
        bool preyDead = false;
        if (bestPrey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            preyDead = herbivore.IsDead;
        }

        if (!preyDead && TryCombatActions(bestPrey, herbivore))
        {
            preyWeight = 0f;
            return Vector3.zero;
        }

        if (preyDead && dist <= eatDistance)
        {
            Eat(bestPrey);
            preyWeight = 0f;
            return Vector3.zero;
        }

        preyWeight = 1f - Mathf.Clamp01(dist / genome.preyDetectDistance);

        Vector3 chase = target - pos;
        chase.y = 0f;

        if (preyDead)
        {
            return chase.sqrMagnitude > 0.0001f ? chase.normalized : Vector3.zero;
        }

        Vector3 guidance = chase.normalized;
        Vector3 pnCorrection = ComputeProportionalNavigationVector(pos, target, trackedPreyVelocity);
        Vector3 result = guidance + pnCorrection;
        result.y = 0f;
        return result.sqrMagnitude > 0.0001f ? result.normalized : Vector3.zero;
    }

    GameObject GetBestRememberedPrey()
    {
        GameObject best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < preyObjs.Count; i++)
        {
            GameObject prey = preyObjs[i];
            if (prey == null) continue;
            if (!prey.TryGetComponent<Resource>(out var res)) continue;

            float dist = Vector3.Distance(transform.position, prey.transform.position);
            if (dist > genome.disengageDistance) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = prey;
            }
        }

        return best;
    }

    Vector3 ComputeThreatVector(out float threatWeight)
    {
        Vector3 pos = transform.position;
        Vector3 away = Vector3.zero;
        threatWeight = 0f;

        for (int i = 0; i < threatObjs.Count; i++)
        {
            GameObject threat = threatObjs[i];
            if (threat == null) continue;

            Vector3 toThreat = threat.transform.position - pos;
            toThreat.y = 0f;
            float dist = toThreat.magnitude;
            if (dist <= 0.001f) continue;

            float strength = ComputeThreatLevel(dist);
            away += (-toThreat / dist) * strength;
            if (strength > threatWeight) threatWeight = strength;
        }

        away.y = 0f;
        return away;
    }

    float ComputeThreatLevel(float dist)
    {
        return AnimalAICommon.ComputeThreatLevel(dist, genome.threatDetectDistance);
    }

    Vector3 ComputeWanderVector(Vector3 basis)
    {
        Vector3 source = basis.sqrMagnitude > 0.0001f ? basis.normalized : transform.forward;

        float t = Time.time;
        float wave = AnimalAICommon.SumWaveValue(genome.wanderWaves, t);

        if (genome.wanderWaves == null || genome.wanderWaves.Length == 0)
        {
            wave = Mathf.Sin(t * Mathf.Max(0.1f, genome.metabolismRate));
        }

        return Quaternion.Euler(0f, wave * 45f, 0f) * source;
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

    void ClampRotation()
    {
        AnimalAICommon.ClampRotation(transform, terrain);
    }

    bool TryCombatActions(GameObject prey, herbivoreBehaviour herbivore)
    {
        if (prey == null || herbivore == null || herbivore.IsDead || !CanAttackLivePrey())
            return false;
        if (!TryGetComponent<Collider>(out var selfCollider))
            return false;
        if (!prey.TryGetComponent<Collider>(out var preyCollider))
            return false;

        if (threatMap == null)
            threatMap = FindFirstObjectByType<threatmap_calc>();

        var context = new PredatorCombatLibrary.CombatContext
        {
            attacker = transform,
            attackerCollider = selfCollider,
            attackerVelocity = currentVelocity,
            targetPosition = prey.transform.position,
            targetVelocity = herbivore.CurrentVelocity,
            targetForward = prey.transform.forward,
            currentTime = Time.time,
            threatMap = threatMap,
            threatPulsePosition = new Vector2(transform.position.x, transform.position.z)
        };

        PredatorCombatLibrary.CombatResult result =
            PredatorCombatLibrary.TryCombatActions(genome, context, combatState, CanAttackLivePrey(), preyCollider);
        combatState = result.nextState;
        if (!result.performed)
            return false;

        herbivore.TakeDamage(result.damage);
        energy -= result.energyCost;
        ClampEnergy();

        if (result.copyTargetVelocity)
        {
            currentVelocity = result.inheritedVelocity;
            pendingMoveVector = result.inheritedMoveDirection;
        }

        return true;
    }

    void Eat(GameObject prey)
    {
        if (prey == null) return;
        if (!prey.TryGetComponent<Resource>(out var resource)) return;
        if (resource.resourceCategory != Resource.category.herbivore) return;
        if (!prey.TryGetComponent<herbivoreBehaviour>(out var herbivore) || !herbivore.IsDead) return;

        bodyResource.Eating(genome.eatspeed * Time.deltaTime, resource);
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

    bool CanAttackLivePrey()
    {
        if (maxEnergy <= 0f)
            return true;

        return energy / maxEnergy > 0.1f;
    }

    void UpdateTrackedPreyMotion(GameObject prey, Vector3 preyPosition)
    {
        if (trackedPrey != prey)
        {
            trackedPrey = prey;
            lastTrackedPreyPosition = preyPosition;
            trackedPreyVelocity = Vector3.zero;
            hasTrackedPreySample = false;
            return;
        }

        float dt = Mathf.Max(Time.deltaTime, 0.0001f);
        trackedPreyVelocity = (preyPosition - lastTrackedPreyPosition) / dt;
        trackedPreyVelocity.y = 0f;
        lastTrackedPreyPosition = preyPosition;
        hasTrackedPreySample = true;
    }

    Vector3 ComputeProportionalNavigationVector(Vector3 selfPosition, Vector3 targetPosition, Vector3 targetVelocity)
    {
        Vector3 lineOfSight = targetPosition - selfPosition;
        lineOfSight.y = 0f;
        float losSqrMag = lineOfSight.sqrMagnitude;
        if (!hasTrackedPreySample || losSqrMag <= 0.0001f)
            return Vector3.zero;

        Vector3 selfVelocity = currentVelocity;
        selfVelocity.y = 0f;

        Vector3 relativeVelocity = targetVelocity - selfVelocity;
        float losRate = Vector3.Cross(lineOfSight, relativeVelocity).y / losSqrMag;
        float closingSpeed = Mathf.Max(0f, Vector3.Dot(-relativeVelocity, lineOfSight.normalized));
        if (closingSpeed <= 0.001f)
            return Vector3.zero;

        float navigationConstant = 2f;
        float correctionStrength = Mathf.Clamp(losRate * closingSpeed * navigationConstant, -1f, 1f);
        if (Mathf.Abs(correctionStrength) <= 0.0001f)
            return Vector3.zero;

        Vector3 lateral = Vector3.Cross(Vector3.up, lineOfSight.normalized);
        return lateral * correctionStrength;
    }
}

