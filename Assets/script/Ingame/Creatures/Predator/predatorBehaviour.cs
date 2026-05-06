using System.Collections.Generic;

using UnityEngine;

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



    [Header("Mana")]

    public float maxMana = 120f;

    public float mana = 120f;



    [Header("Sensing")]

    public float eatDistance = 1.2f;

    [Header("Field Mana")]

    public float manaAbsorbFromFieldPerSec = 1f;

    public float fieldAbsorbRadius = 2f;

    public bool isConvertFieldAbsorb;

    public float fieldAbsorbLogScale = 1f;

    float nextFieldAbsorbTime;

    [Header("Threat Pulse")]

    public float presenceThreatPulseInterval = 0.5f;

    float nextPresenceThreatPulseTime;

    threatmap_calc cachedThreatMap;

    [Header("Phase")]

    public float phaseCheckInterval = 10f;

    public float phaseUpManaCoefficient = 0.00001f;

    public float phaseUpProbabilityCap = 0.005f;

    float nextPhaseCheckTime;



    List<(GameObject obj, float time)> memoryPrey = new();

    List<(GameObject obj, float time)> memoryThreat = new();

    List<GameObject> preyObjs = new();

    List<GameObject> threatObjs = new();



    GameObject trackedPrey;

    Vector3 lastTrackedPreyPosition;

    Vector3 trackedPreyVelocity;

    Vector3 currentVelocity;
    Vector3 inertialMoveVector;
    Vector3 inertialFacingVector;

    bool hasTrackedPreySample;

    bool isMovementSuppressed;

    AnimalAICommon.MovementTelemetry lastMovementTelemetry;

    Vector3 pendingMoveVector;

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

        EnsureCollisionProfile();

        bodyResource = GetComponent<Resource>();

        resourceDispenser = ResourceDispenser.Instance != null ? ResourceDispenser.Instance : FindFirstObjectByType<ResourceDispenser>();

        var gauge = GetComponent<CreatureVirtualGauge>();

        if (gauge == null)

            gauge = gameObject.AddComponent<CreatureVirtualGauge>();

        gauge.Initialize(this);

        maxHealth = CreatureBalanceTuning.PredatorMaxHealth;

        health = maxHealth;

        SyncManaFromResource();

        ClampMana();

        EnsurePredatorPhase();

        nextPhaseCheckTime = Time.time + phaseCheckInterval;

    }



    void EnsureCollisionProfile()

    {

        CreatureCollisionProfile profile = GetComponent<CreatureCollisionProfile>();

        if (profile == null)

            profile = gameObject.AddComponent<CreatureCollisionProfile>();

        profile.ApplyDefaults("predator");

    }



    void OnDestroy()

    {

        if (predatorManager != null)

            predatorManager.Unregister(gameObject);

    }



    void Update()

    {

        if (bodyResource == null) return;

        SyncManaFromResource();

        TryAbsorbManaFromField();

        TryPhaseEvolution();



        if (IsDead)

        {

            pendingMoveVector = Vector3.zero;

            bodyResource.Decompose(GetDecomposeRate(), resourceDispenser);

            return;

        }

        EmitPresenceThreatPulse();



        UpdateVision();



        pendingMoveVector = ComputeTotalVector();

    }



    void FixedUpdate()

    {

        if (bodyResource != null && !IsDead)

        {

            lastMovementTelemetry = ApplyMovement(pendingMoveVector);

        }



        ClampRotation();

    }



    public void TakeDamage(float amount)

    {

        if (IsDead || amount <= 0f) return;



        float appliedDamage = Mathf.Min(health, amount);

        health -= amount;

        DamageNumberLibrary.ShowDamage(transform.position, appliedDamage);

        if (health <= 0f)

        {

            health = 0f;

            currentTarget = null;

            pendingMoveVector = Vector3.zero;

            currentVelocity = Vector3.zero;

            currentSpeed = 0f;

            inertialMoveVector = Vector3.zero;

            inertialFacingVector = Vector3.zero;

            AnimalAICommon.PrepareCorpseRigidbody(gameObject);

        }

    }



    float GetDecomposeRate()

    {

        return resourceDispenser != null ? resourceDispenser.decomposeRate : 2f;

    }



    void ClampMana()

    {

        mana = Mathf.Max(0f, mana);

    }

    void SyncManaFromResource()
    {
        if (bodyResource == null) return;
        if (bodyResource.maxMana > 0f)
            maxMana = bodyResource.maxMana;
        mana = bodyResource.mana;
    }

    void TryAbsorbManaFromField()
    {
        if (bodyResource == null || IsDead || Time.time < nextFieldAbsorbTime)
            return;

        nextFieldAbsorbTime = Time.time + 1f;
        bodyResource.AbsorbManaFromField(manaAbsorbFromFieldPerSec, fieldAbsorbRadius, isConvertFieldAbsorb, fieldAbsorbLogScale);
        SyncManaFromResource();
    }

    void EnsurePredatorPhase()
    {
        if (bodyResource == null)
            return;

        if (GetPhaseRank(bodyResource.resourceCategory) < 3)
            bodyResource.resourceCategory = category.predator;
    }

    void TryPhaseEvolution()
    {
        if (bodyResource == null || IsDead)
            return;

        EnsurePredatorPhase();

        if (Time.time < nextPhaseCheckTime)
            return;

        nextPhaseCheckTime = Time.time + Mathf.Max(0.1f, phaseCheckInterval);

        int currentRank = GetPhaseRank(bodyResource.resourceCategory);
        if (currentRank < 3 || currentRank >= 5)
            return;

        float fieldMana = ManaFieldManager.GetOrCreate().SampleMana(transform.position);
        float probability = Mathf.Min(Mathf.Max(0f, phaseUpProbabilityCap), Mathf.Max(0f, fieldMana) * Mathf.Max(0f, phaseUpManaCoefficient));
        if (Random.value > probability)
            return;

        bodyResource.resourceCategory = GetCategoryFromPhaseRank(currentRank + 1);
        bodyResource.speciesID = DrawPhaseUpSpeciesID();
        bodyResource.RecordManaEvent("phase up " + bodyResource.resourceCategory + " speciesID=" + bodyResource.speciesID, 0f);
    }

    static int GetPhaseRank(category value)
    {
        switch (value)
        {
            case category.grass:
                return 1;
            case category.herbivore:
                return 2;
            case category.predator:
                return 3;
            case category.highpredator:
                return 4;
            case category.dominant:
                return 5;
            default:
                return 0;
        }
    }

    static category GetCategoryFromPhaseRank(int rank)
    {
        switch (rank)
        {
            case 1:
                return category.grass;
            case 2:
                return category.herbivore;
            case 3:
                return category.predator;
            case 4:
                return category.highpredator;
            case 5:
                return category.dominant;
            default:
                return category.predator;
        }
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

            if (!CanTargetAsPrey(res)) continue;



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



        if (predatorManager == null)
            return;

        Vector3 pos = transform.position;

        if (predatorManager.returnHerbivores(out List<GameObject> herbivores))
        {
            foreach (var prey in herbivores)
                TryRememberPrey(prey, pos);
        }

        if (predatorManager != null)
        {
            for (int i = 0; i < predatorManager.predators.Count; i++)
                TryRememberPrey(predatorManager.predators[i], pos);
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

    void TryRememberPrey(GameObject prey, Vector3 selfPosition)
    {
        if (prey == null || prey == gameObject) return;
        if (!prey.TryGetComponent<Resource>(out var res)) return;
        if (!CanTargetAsPrey(res)) return;

        float dist = Vector3.Distance(prey.transform.position, selfPosition);
        if (dist <= 0.001f || dist > genome.preyDetectDistance) return;

        Remember(memoryPrey, prey);
    }

    bool CanTargetAsPrey(Resource target)
    {
        if (target == null || bodyResource == null || target == bodyResource)
            return false;

        int selfRank = GetPhaseRank(bodyResource.resourceCategory);
        int targetRank = GetPhaseRank(target.resourceCategory);
        if (selfRank < 3)
            return false;

        if (targetRank >= 3 && target.speciesID == bodyResource.speciesID)
            return false;

        return targetRank >= 2;
    }

    int DrawPhaseUpSpeciesID()
    {
        int maxSpeciesID = 0;
        Resource[] resources = FindObjectsByType<Resource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i] == null) continue;
            maxSpeciesID = Mathf.Max(maxSpeciesID, resources[i].speciesID);
        }

        int candidateMax = maxSpeciesID + 1;
        float totalWeight = 0f;
        for (int id = 0; id <= candidateMax; id++)
            totalWeight += 1f / (id + 1f);

        float roll = Random.value * totalWeight;
        for (int id = 0; id <= candidateMax; id++)
        {
            roll -= 1f / (id + 1f);
            if (roll <= 0f)
                return id;
        }

        return candidateMax;
    }

    bool IsPreyDead(GameObject prey)
    {
        if (prey == null)
            return true;
        if (prey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return herbivore.IsDead;
        if (prey.TryGetComponent<predatorBehaviour>(out var predator))
            return predator.IsDead;
        return false;
    }

    Vector3 GetPreyVelocity(GameObject prey)
    {
        if (prey == null)
            return Vector3.zero;
        if (prey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return herbivore.CurrentVelocity;
        if (prey.TryGetComponent<predatorBehaviour>(out var predator))
            return predator.CurrentVelocity;
        return Vector3.zero;
    }

    void ApplyDamageToPrey(GameObject prey, float damage)
    {
        if (prey == null)
            return;
        if (prey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            herbivore.TakeDamage(damage);
            return;
        }
        if (prey.TryGetComponent<predatorBehaviour>(out var predator))
            predator.TakeDamage(damage);
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

        bool preyDead = IsPreyDead(bestPrey);



        if (!preyDead && TryCombatActions(bestPrey))

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

                ref inertialMoveVector,

                ref inertialFacingVector,

                genome.forwardForce,

                genome.turnForce,

                Time.fixedDeltaTime);



        return AnimalAICommon.ApplyMovement(

            transform,

            terrain,

            total,

            ref currentSpeed,

            ref currentVelocity,

            ref inertialMoveVector,

            ref inertialFacingVector,

            genome.forwardForce * movementCapacity,

            genome.turnForce,

            Time.fixedDeltaTime);

    }



    void ClampRotation()

    {

        AnimalAICommon.ClampRotation(transform, terrain);

    }



    bool TryCombatActions(GameObject prey)

    {

        if (prey == null || IsPreyDead(prey) || !CanAttackLivePrey())

            return false;

        if (!TryGetComponent<Collider>(out var selfCollider))

            return false;

        if (!prey.TryGetComponent<Collider>(out var preyCollider))

            return false;


        var context = new PredatorCombatLibrary.CombatContext

        {

            attacker = transform,

            attackerCollider = selfCollider,

            attackerVelocity = currentVelocity,

            targetPosition = prey.transform.position,

            targetVelocity = GetPreyVelocity(prey),

            targetForward = prey.transform.forward,

            currentTime = Time.time

        };



        PredatorCombatLibrary.CombatResult result =

            PredatorCombatLibrary.TryCombatActions(genome, context, combatState, CanAttackLivePrey(), preyCollider);

        combatState = result.nextState;

        if (!result.performed)

            return false;



        ApplyDamageToPrey(prey, result.damage);

        EmitAttackThreatPulse(prey.transform.position);

        bodyResource.AddMana(result.damage, out _, "attack drain");
        bodyResource.RemoveMana(result.manaCost, "attack cost");
        SyncManaFromResource();



        if (result.copyTargetVelocity)

        {

            currentVelocity = result.inheritedVelocity;

            pendingMoveVector = result.inheritedMoveDirection;

        }



        return true;

    }

    void EmitAttackThreatPulse(Vector3 point)
    {
        if (genome.attackThreatPulseScore <= 0f || genome.attackThreatPulseRadius <= 0f)
            return;

        threatmap_calc threatMap = GetThreatMap();
        if (threatMap == null)
            return;

        threatMap.AddThreatPulse(point, genome.attackThreatPulseScore, genome.attackThreatPulseRadius);
    }

    void EmitPresenceThreatPulse()
    {
        if (Time.time < nextPresenceThreatPulseTime)
            return;

        nextPresenceThreatPulseTime = Time.time + Mathf.Max(0.02f, presenceThreatPulseInterval);

        if (genome.attackThreatPulseScore <= 0f || genome.attackThreatPulseRadius <= 0f)
            return;

        threatmap_calc threatMap = GetThreatMap();
        if (threatMap == null)
            return;

        threatMap.AddThreatPulse(transform.position, genome.attackThreatPulseScore, genome.attackThreatPulseRadius);
    }

    threatmap_calc GetThreatMap()
    {
        if (cachedThreatMap == null)
            cachedThreatMap = FindFirstObjectByType<threatmap_calc>();

        return cachedThreatMap;
    }



    void Eat(GameObject prey)

    {

        if (prey == null) return;

        if (!prey.TryGetComponent<Resource>(out var resource)) return;

        if (!CanTargetAsPrey(resource)) return;

        if (!IsPreyDead(prey)) return;



        bodyResource.Eating(genome.eatspeed * Time.deltaTime, resource, "corpse eat");

        SyncManaFromResource();

    }



    float GetMovementCapacity()

    {

        if (maxMana <= 0f)

            return 1f;



        float ratio = Mathf.Clamp01(mana / maxMana);

        if (ratio <= 0.05f)

            return 0f;



        return Mathf.InverseLerp(0.05f, 0.25f, ratio);

    }



    bool CanAttackLivePrey()

    {

        if (maxMana <= 0f)

            return true;



        return mana / maxMana > 0.1f;

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



