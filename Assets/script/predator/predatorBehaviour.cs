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

    public WaveGene[] visionWaves;
    public WaveGene[] wanderWaves;
}

[RequireComponent(typeof(Rigidbody))]
public class predatorBehaviour : MonoBehaviour
{
    public predatorManager predatorManager;
    public PredatorGenome genome;
    public Terrain terrain;

    public Resource bodyResource;
    ResourceDispenser resourceDispenser;

    Rigidbody rb;

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

    float lastAttackTime = -999f;

    public Vector2? currentTarget;
    public bool IsDead => health <= 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bodyResource = GetComponent<Resource>();
        resourceDispenser = ResourceDispenser.Instance != null ? ResourceDispenser.Instance : FindFirstObjectByType<ResourceDispenser>();
        var gauge = GetComponent<CreatureVirtualGauge>();
        if (gauge == null)
            gauge = gameObject.AddComponent<CreatureVirtualGauge>();
        gauge.Initialize(this);
        health = maxHealth;
        energy = maxEnergy;
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
            bodyResource.Decompose(GetDecomposeRate(), resourceDispenser);
            return;
        }

        ConvertBodyCarbonToEnergy();
        UpdateVision();

        Vector3 moveVec = ComputeTotalVector();
        ApplyMovement(moveVec);
        ConsumeEnergy(moveVec);
    }

    void FixedUpdate()
    {
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
            rb.linearVelocity = Vector3.zero;
        }
    }

    void ConvertBodyCarbonToEnergy()
    {
        float generated = GetCarbonToEnergyRate() * Time.deltaTime;
        if (generated <= 0f) return;

        energy = Mathf.Min(maxEnergy, energy + generated);
    }

    void ConsumeEnergy(Vector3 moveVec)
    {
        float moveFactor = Mathf.Clamp01(moveVec.magnitude);
        float cost = (GetIdleEnergyCostPerSec() + GetMoveEnergyCostPerSec() * moveFactor) * Time.deltaTime;
        energy = Mathf.Max(0f, energy - cost);
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

        Debug.DrawLine(transform.position, transform.position + vPrey, Color.red);
        Debug.DrawLine(transform.position, transform.position + vThreat, Color.magenta);
        Debug.DrawLine(transform.position, transform.position + vBoundary, Color.blue);
        Debug.DrawLine(transform.position, transform.position + vWander, Color.green);

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

        float dist = Vector3.Distance(pos, target);
        float attackRange = Mathf.Max(eatDistance, genome.attackDistance);

        bool preyDead = false;
        if (bestPrey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            preyDead = herbivore.IsDead;
        }

        if (dist <= attackRange)
        {
            if (preyDead)
            {
                Eat(bestPrey);
            }
            else
            {
                Attack(bestPrey);
            }

            preyWeight = 1f;
            Vector3 brake = pos - target;
            brake.y = 0f;
            return brake.sqrMagnitude > 0.0001f ? brake.normalized * 0.1f : Vector3.zero;
        }

        preyWeight = 1f - Mathf.Clamp01(dist / genome.preyDetectDistance);

        Vector3 chase = target - pos;
        chase.y = 0f;

        float preferred = Mathf.Max(0.1f, genome.preferredChaseDistance);
        if (dist < preferred)
        {
            return -chase.normalized * ((preferred - dist) / preferred);
        }

        return chase.normalized;
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
            float dist = toThreat.magnitude;
            if (dist <= 0.001f) continue;

            float strength = ComputeThreatLevel(dist);
            away += (-toThreat / dist) * strength;
            if (strength > threatWeight) threatWeight = strength;
        }

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

    void ApplyMovement(Vector3 total)
    {
        AnimalAICommon.ApplyMovement(rb, total, ref currentSpeed, genome.forwardForce, genome.turnForce);
    }

    void ClampRotation()
    {
        AnimalAICommon.ClampRotation(rb, 45f, 5f);
    }

    void Attack(GameObject prey)
    {
        if (Time.time < lastAttackTime + genome.attackCooldown) return;

        if (prey.TryGetComponent<herbivoreBehaviour>(out var herbivore) && !herbivore.IsDead)
        {
            herbivore.TakeDamage(genome.attackDamage);
            lastAttackTime = Time.time;
        }
    }

    void Eat(GameObject prey)
    {
        if (prey == null) return;
        if (!prey.TryGetComponent<Resource>(out var resource)) return;
        if (resource.resourceCategory != Resource.category.herbivore) return;
        if (!prey.TryGetComponent<herbivoreBehaviour>(out var herbivore) || !herbivore.IsDead) return;

        bodyResource.Eating(genome.eatspeed * Time.deltaTime, resource);
    }
}

