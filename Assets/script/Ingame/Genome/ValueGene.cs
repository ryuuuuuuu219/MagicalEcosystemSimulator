using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ValueGene
{
    public SpeciesType species;
    public category phase;
    public int speciesID;
    public LegacyGenomeValueGene Legacy;
    public GroundMotorValueGene GroundMotor;
    public FoodVisionSenseValueGene FoodVisionSense;
    public PredatorVisionSenseValueGene PredatorVisionSense;
    public PreyVisionSenseValueGene PreyVisionSense;
    public ThreatVisionSenseValueGene ThreatVisionSense;
    public MemoryValueGene Memory;
    public EatActionValueGene GrassEatAction;
    public EatActionValueGene CorpseEatAction;
    public DesireValueGene FoodDesire;
    public DesireValueGene PreyChaseDesire;
    public DesireValueGene ThreatAvoidanceDesire;
    public DesireValueGene WanderDesire;
    public RandomEvasionActionValueGene RandomEvasionAction;
    public ChargeAttackActionValueGene ChargeAttackAction;
    public BiteAttackActionValueGene BiteAttackAction;
    public MeleeAttackActionValueGene MeleeAttackAction;
    public ThreatPulseEmitterValueGene ThreatPulseEmitter;
    public ManaActionValueGene FieldManaAbsorbAction;
    public ManaFieldSenseValueGene ManaFieldSense;
    public MagicValueGene Magic;
    public WaveGene[] visionWaves;
    public WaveGene[] wanderWaves;

    public static ValueGene FromHerbivoreGenome(HerbivoreGenome genome, category phase = category.herbivore, int speciesID = 0)
    {
        return new ValueGene
        {
            species = SpeciesType.Herbivore,
            phase = phase,
            speciesID = speciesID,
            Legacy = new LegacyGenomeValueGene
            {
                metabolismRate = genome.metabolismRate,
                corpseWeight = genome.corpseWeight
            },
            GroundMotor = new GroundMotorValueGene
            {
                forwardForce = genome.forwardForce,
                turnForce = genome.turnForce
            },
            FoodVisionSense = new FoodVisionSenseValueGene
            {
                visionDistance = genome.visionDistance,
                visionAngle = genome.visionAngle,
                visionTurnAngle = genome.visionturnAngle
            },
            PredatorVisionSense = new PredatorVisionSenseValueGene
            {
                threatDetectDistance = genome.threatDetectDistance,
                visionAngle = genome.visionAngle
            },
            ThreatVisionSense = new ThreatVisionSenseValueGene
            {
                threatDetectDistance = genome.threatDetectDistance
            },
            Memory = new MemoryValueGene
            {
                memoryTime = genome.memorytime
            },
            GrassEatAction = new EatActionValueGene
            {
                eatSpeed = genome.eatspeed
            },
            CorpseEatAction = new EatActionValueGene
            {
                eatSpeed = genome.eatspeed
            },
            FoodDesire = new DesireValueGene
            {
                level = Mathf.Max(0f, genome.foodWeight),
                weight = Mathf.Max(0f, genome.foodWeight)
            },
            ThreatAvoidanceDesire = new DesireValueGene
            {
                level = Mathf.Max(0f, genome.fearThreshold),
                weight = Mathf.Max(0f, genome.predatorWeight),
                distance = genome.runAwayDistance,
                threshold = genome.escapeThreshold
            },
            WanderDesire = new DesireValueGene
            {
                level = Mathf.Max(0f, genome.curiosity),
                weight = Mathf.Max(0f, genome.curiosity)
            },
            RandomEvasionAction = new RandomEvasionActionValueGene
            {
                contactEscapeDistance = genome.contactEscapeDistance,
                evasionAngle = genome.evasionAngle,
                evasionDuration = genome.evasionDuration,
                evasionCooldown = genome.evasionCooldown,
                evasionDistance = genome.evasionDistance,
                predictIntercept = genome.predictIntercept,
                zigzagFrequency = genome.zigzagFrequency,
                zigzagAmplitude = genome.zigzagAmplitude
            },
            FieldManaAbsorbAction = ManaActionValueGene.Default,
            ManaFieldSense = ManaFieldSenseValueGene.Default,
            Magic = MagicValueGene.Default,
            visionWaves = CloneWaves(genome.visionWaves),
            wanderWaves = CloneWaves(genome.wanderWaves)
        };
    }

    public static ValueGene FromPredatorGenome(PredatorGenome genome, category phase = category.predator, int speciesID = 0)
    {
        return new ValueGene
        {
            species = SpeciesType.Predator,
            phase = phase,
            speciesID = speciesID,
            Legacy = new LegacyGenomeValueGene
            {
                metabolismRate = genome.metabolismRate,
                attackDistance = genome.attackDistance,
                attackDamage = genome.attackDamage,
                attackCooldown = genome.attackCooldown
            },
            GroundMotor = new GroundMotorValueGene
            {
                forwardForce = genome.forwardForce,
                turnForce = genome.turnForce
            },
            PreyVisionSense = new PreyVisionSenseValueGene
            {
                preyDetectDistance = genome.preyDetectDistance,
                visionDistance = genome.visionDistance,
                visionAngle = genome.visionAngle,
                visionTurnAngle = genome.visionTurnAngle
            },
            ThreatVisionSense = new ThreatVisionSenseValueGene
            {
                threatDetectDistance = genome.threatDetectDistance
            },
            Memory = new MemoryValueGene
            {
                memoryTime = genome.memorytime
            },
            CorpseEatAction = new EatActionValueGene
            {
                eatSpeed = genome.eatspeed
            },
            PreyChaseDesire = new DesireValueGene
            {
                level = Mathf.Max(0f, genome.chaseWeight),
                weight = Mathf.Max(0f, genome.chaseWeight),
                distance = genome.preferredChaseDistance,
                disengageDistance = genome.disengageDistance,
                stopMoveThreshold = genome.stopMoveThreshold,
                resumeMoveThreshold = genome.resumeMoveThreshold
            },
            ThreatAvoidanceDesire = new DesireValueGene
            {
                level = Mathf.Max(0f, genome.threatWeight),
                weight = Mathf.Max(0f, genome.threatWeight)
            },
            ChargeAttackAction = new ChargeAttackActionValueGene
            {
                arc = genome.chargeArc,
                damageScale = genome.chargeDamageScale,
                manaCost = genome.chargeManaCost,
                contactPadding = genome.chargeContactPadding,
                attackClock = genome.chargeAttackClock
            },
            BiteAttackAction = new BiteAttackActionValueGene
            {
                arc = genome.biteArc,
                damage = genome.biteDamage,
                manaCost = genome.biteManaCost,
                attackClock = genome.biteAttackClock
            },
            MeleeAttackAction = new MeleeAttackActionValueGene
            {
                arc = genome.meleeArc,
                damage = genome.meleeDamage,
                manaCost = genome.meleeManaCost,
                attackClock = genome.meleeAttackClock
            },
            ThreatPulseEmitter = new ThreatPulseEmitterValueGene
            {
                pulseScore = genome.attackThreatPulseScore,
                pulseRadius = genome.attackThreatPulseRadius,
                interval = 0.5f,
                traceScale = genome.attackTraceScale,
                traceDuration = genome.attackTraceDuration,
                traceDepth = genome.attackTraceDepth
            },
            FieldManaAbsorbAction = ManaActionValueGene.Default,
            ManaFieldSense = ManaFieldSenseValueGene.Default,
            Magic = MagicValueGene.Default,
            visionWaves = CloneWaves(genome.visionWaves),
            wanderWaves = CloneWaves(genome.wanderWaves)
        };
    }

    public HerbivoreGenome ToHerbivoreGenome(HerbivoreGenome fallback)
    {
        fallback.forwardForce = PositiveOrFallback(GroundMotor.forwardForce, fallback.forwardForce);
        fallback.turnForce = PositiveOrFallback(GroundMotor.turnForce, fallback.turnForce);
        fallback.metabolismRate = NonNegativeOrFallback(Legacy.metabolismRate, fallback.metabolismRate);
        fallback.visionDistance = PositiveOrFallback(FoodVisionSense.visionDistance, fallback.visionDistance);
        fallback.visionAngle = PositiveOrFallback(FoodVisionSense.visionAngle, fallback.visionAngle);
        fallback.visionturnAngle = PositiveOrFallback(FoodVisionSense.visionTurnAngle, fallback.visionturnAngle);
        fallback.eatspeed = PositiveOrFallback(GrassEatAction.eatSpeed, fallback.eatspeed);
        fallback.threatDetectDistance = PositiveOrFallback(PredatorVisionSense.threatDetectDistance, fallback.threatDetectDistance);
        fallback.memorytime = PositiveOrFallback(Memory.memoryTime, fallback.memorytime);
        fallback.runAwayDistance = PositiveOrFallback(ThreatAvoidanceDesire.distance, fallback.runAwayDistance);
        fallback.contactEscapeDistance = PositiveOrFallback(RandomEvasionAction.contactEscapeDistance, fallback.contactEscapeDistance);
        fallback.evasionAngle = PositiveOrFallback(RandomEvasionAction.evasionAngle, fallback.evasionAngle);
        fallback.evasionDuration = PositiveOrFallback(RandomEvasionAction.evasionDuration, fallback.evasionDuration);
        fallback.evasionCooldown = PositiveOrFallback(RandomEvasionAction.evasionCooldown, fallback.evasionCooldown);
        fallback.evasionDistance = PositiveOrFallback(RandomEvasionAction.evasionDistance, fallback.evasionDistance);
        fallback.predictIntercept = RandomEvasionAction.predictIntercept;
        fallback.zigzagFrequency = PositiveOrFallback(RandomEvasionAction.zigzagFrequency, fallback.zigzagFrequency);
        fallback.zigzagAmplitude = NonNegativeOrFallback(RandomEvasionAction.zigzagAmplitude, fallback.zigzagAmplitude);
        fallback.foodWeight = PositiveOrFallback(FoodDesire.weight, fallback.foodWeight);
        fallback.predatorWeight = PositiveOrFallback(ThreatAvoidanceDesire.weight, fallback.predatorWeight);
        fallback.corpseWeight = PositiveOrFallback(Legacy.corpseWeight, fallback.corpseWeight);
        fallback.fearThreshold = PositiveOrFallback(ThreatAvoidanceDesire.level, fallback.fearThreshold);
        fallback.escapeThreshold = PositiveOrFallback(ThreatAvoidanceDesire.threshold, fallback.escapeThreshold);
        if (visionWaves != null && visionWaves.Length > 0)
            fallback.visionWaves = CloneWaves(visionWaves);
        if (wanderWaves != null && wanderWaves.Length > 0)
            fallback.wanderWaves = CloneWaves(wanderWaves);
        return fallback;
    }

    public PredatorGenome ToPredatorGenome(PredatorGenome fallback)
    {
        fallback.forwardForce = PositiveOrFallback(GroundMotor.forwardForce, fallback.forwardForce);
        fallback.turnForce = PositiveOrFallback(GroundMotor.turnForce, fallback.turnForce);
        fallback.metabolismRate = NonNegativeOrFallback(Legacy.metabolismRate, fallback.metabolismRate);
        fallback.visionDistance = PositiveOrFallback(PreyVisionSense.visionDistance, fallback.visionDistance);
        fallback.visionAngle = PositiveOrFallback(PreyVisionSense.visionAngle, fallback.visionAngle);
        fallback.visionTurnAngle = PositiveOrFallback(PreyVisionSense.visionTurnAngle, fallback.visionTurnAngle);
        fallback.eatspeed = PositiveOrFallback(CorpseEatAction.eatSpeed, fallback.eatspeed);
        fallback.chaseWeight = PositiveOrFallback(PreyChaseDesire.weight, fallback.chaseWeight);
        fallback.preyDetectDistance = PositiveOrFallback(PreyVisionSense.preyDetectDistance, fallback.preyDetectDistance);
        fallback.attackDistance = PositiveOrFallback(Legacy.attackDistance, fallback.attackDistance);
        fallback.attackDamage = PositiveOrFallback(Legacy.attackDamage, fallback.attackDamage);
        fallback.attackCooldown = PositiveOrFallback(Legacy.attackCooldown, fallback.attackCooldown);
        fallback.threatWeight = PositiveOrFallback(ThreatAvoidanceDesire.weight, fallback.threatWeight);
        fallback.threatDetectDistance = PositiveOrFallback(ThreatVisionSense.threatDetectDistance, fallback.threatDetectDistance);
        fallback.memorytime = PositiveOrFallback(Memory.memoryTime, fallback.memorytime);
        fallback.preferredChaseDistance = PositiveOrFallback(PreyChaseDesire.distance, fallback.preferredChaseDistance);
        fallback.disengageDistance = PositiveOrFallback(PreyChaseDesire.disengageDistance, fallback.disengageDistance);
        fallback.stopMoveThreshold = PositiveOrFallback(PreyChaseDesire.stopMoveThreshold, fallback.stopMoveThreshold);
        fallback.resumeMoveThreshold = PositiveOrFallback(PreyChaseDesire.resumeMoveThreshold, fallback.resumeMoveThreshold);
        fallback.chargeArc = ChargeAttackAction.arc;
        fallback.chargeDamageScale = PositiveOrFallback(ChargeAttackAction.damageScale, fallback.chargeDamageScale);
        fallback.chargeManaCost = NonNegativeOrFallback(ChargeAttackAction.manaCost, fallback.chargeManaCost);
        fallback.chargeContactPadding = NonNegativeOrFallback(ChargeAttackAction.contactPadding, fallback.chargeContactPadding);
        fallback.chargeAttackClock = PositiveOrFallback(ChargeAttackAction.attackClock, fallback.chargeAttackClock);
        fallback.biteArc = BiteAttackAction.arc;
        fallback.biteDamage = PositiveOrFallback(BiteAttackAction.damage, fallback.biteDamage);
        fallback.biteManaCost = NonNegativeOrFallback(BiteAttackAction.manaCost, fallback.biteManaCost);
        fallback.biteAttackClock = PositiveOrFallback(BiteAttackAction.attackClock, fallback.biteAttackClock);
        fallback.meleeArc = MeleeAttackAction.arc;
        fallback.meleeDamage = PositiveOrFallback(MeleeAttackAction.damage, fallback.meleeDamage);
        fallback.meleeManaCost = NonNegativeOrFallback(MeleeAttackAction.manaCost, fallback.meleeManaCost);
        fallback.meleeAttackClock = PositiveOrFallback(MeleeAttackAction.attackClock, fallback.meleeAttackClock);
        fallback.attackThreatPulseScore = PositiveOrFallback(ThreatPulseEmitter.pulseScore, fallback.attackThreatPulseScore);
        fallback.attackThreatPulseRadius = PositiveOrFallback(ThreatPulseEmitter.pulseRadius, fallback.attackThreatPulseRadius);
        fallback.attackTraceScale = PositiveOrFallback(ThreatPulseEmitter.traceScale, fallback.attackTraceScale);
        fallback.attackTraceDuration = PositiveOrFallback(ThreatPulseEmitter.traceDuration, fallback.attackTraceDuration);
        fallback.attackTraceDepth = PositiveOrFallback(ThreatPulseEmitter.traceDepth, fallback.attackTraceDepth);
        if (visionWaves != null && visionWaves.Length > 0)
            fallback.visionWaves = CloneWaves(visionWaves);
        if (wanderWaves != null && wanderWaves.Length > 0)
            fallback.wanderWaves = CloneWaves(wanderWaves);
        return fallback;
    }

    public ValueGene Clone()
    {
        return new ValueGene
        {
            species = species,
            phase = phase,
            speciesID = speciesID,
            Legacy = Legacy,
            GroundMotor = GroundMotor,
            FoodVisionSense = FoodVisionSense,
            PredatorVisionSense = PredatorVisionSense,
            PreyVisionSense = PreyVisionSense,
            ThreatVisionSense = ThreatVisionSense,
            Memory = Memory,
            GrassEatAction = GrassEatAction,
            CorpseEatAction = CorpseEatAction,
            FoodDesire = FoodDesire,
            PreyChaseDesire = PreyChaseDesire,
            ThreatAvoidanceDesire = ThreatAvoidanceDesire,
            WanderDesire = WanderDesire,
            RandomEvasionAction = RandomEvasionAction,
            ChargeAttackAction = ChargeAttackAction,
            BiteAttackAction = BiteAttackAction,
            MeleeAttackAction = MeleeAttackAction,
            ThreatPulseEmitter = ThreatPulseEmitter,
            FieldManaAbsorbAction = FieldManaAbsorbAction,
            ManaFieldSense = ManaFieldSense,
            Magic = Magic,
            visionWaves = CloneWaves(visionWaves),
            wanderWaves = CloneWaves(wanderWaves)
        };
    }

    static WaveGene[] CloneWaves(WaveGene[] source)
    {
        if (source == null)
            return null;
        WaveGene[] clone = new WaveGene[source.Length];
        Array.Copy(source, clone, source.Length);
        return clone;
    }

    static float PositiveOrFallback(float value, float fallback)
    {
        return value > 0f ? value : fallback;
    }

    static float NonNegativeOrFallback(float value, float fallback)
    {
        return value >= 0f ? value : fallback;
    }
}

[Serializable]
public struct LegacyGenomeValueGene
{
    public float metabolismRate;
    public float corpseWeight;
    public float attackDistance;
    public float attackDamage;
    public float attackCooldown;
}

[Serializable]
public struct GroundMotorValueGene
{
    public float forwardForce;
    public float turnForce;
}

[Serializable]
public struct FoodVisionSenseValueGene
{
    public float visionDistance;
    public float visionAngle;
    public float visionTurnAngle;
}

[Serializable]
public struct PredatorVisionSenseValueGene
{
    public float threatDetectDistance;
    public float visionAngle;
}

[Serializable]
public struct PreyVisionSenseValueGene
{
    public float preyDetectDistance;
    public float visionDistance;
    public float visionAngle;
    public float visionTurnAngle;
}

[Serializable]
public struct ThreatVisionSenseValueGene
{
    public float threatDetectDistance;
}

[Serializable]
public struct MemoryValueGene
{
    public float memoryTime;
}

[Serializable]
public struct EatActionValueGene
{
    public float eatRadius;
    public float eatSpeed;
}

[Serializable]
public struct DesireValueGene
{
    public float level;
    public float weight;
    public float distance;
    public float threshold;
    public float disengageDistance;
    public float stopMoveThreshold;
    public float resumeMoveThreshold;
}

[Serializable]
public struct RandomEvasionActionValueGene
{
    public float contactEscapeDistance;
    public float evasionAngle;
    public float evasionDuration;
    public float evasionCooldown;
    public float evasionDistance;
    public bool predictIntercept;
    public float zigzagFrequency;
    public float zigzagAmplitude;
}

[Serializable]
public struct ChargeAttackActionValueGene
{
    public AttackArcSettings arc;
    public float damageScale;
    public float manaCost;
    public float contactPadding;
    public float attackClock;
}

[Serializable]
public struct BiteAttackActionValueGene
{
    public AttackArcSettings arc;
    public float damage;
    public float manaCost;
    public float attackClock;
}

[Serializable]
public struct MeleeAttackActionValueGene
{
    public AttackArcSettings arc;
    public float damage;
    public float manaCost;
    public float attackClock;
}

[Serializable]
public struct ThreatPulseEmitterValueGene
{
    public float pulseScore;
    public float pulseRadius;
    public float interval;
    public float traceScale;
    public float traceDuration;
    public float traceDepth;
}

[Serializable]
public struct ManaActionValueGene
{
    public float manaAbsorbPerSec;
    public float absorbRadius;
    public bool convertFieldAbsorb;
    public float logScale;

    public static ManaActionValueGene Default => new ManaActionValueGene
    {
        manaAbsorbPerSec = 1f,
        absorbRadius = 2f,
        convertFieldAbsorb = false,
        logScale = 1f
    };
}

[Serializable]
public struct ManaFieldSenseValueGene
{
    public float level;

    public static ManaFieldSenseValueGene Default => new ManaFieldSenseValueGene
    {
        level = 1f
    };
}

[Serializable]
public struct MagicValueGene
{
    public float attackManaCost;
    public float projectileManaCost;
    public float defenseManaCost;
    public float evasionManaCost;
    public float cooldown;

    public static MagicValueGene Default => new MagicValueGene
    {
        attackManaCost = 10f,
        projectileManaCost = 10f,
        defenseManaCost = 5f,
        evasionManaCost = 5f,
        cooldown = 3f
    };
}

[Serializable]
public class GeneDataSnapshot
{
    public int version = 1;
    public string schema = "GeneDataManager";
    public List<ValueGene> genes_v = new();
    public List<AIComponentGene> genes_s = new();
    public List<GeneDataRecord> records = new();
    public List<GeneDataRecord> checkpoints = new();
}

[Serializable]
public class GeneDataRecord
{
    public int instanceId;
    public string objectName;
    public SpeciesType species;
    public category phase;
    public int speciesID;
    public bool fromCheckpoint;
    public string checkpointReason;
    public float checkpointTime;
    public ValueGene valueGene;
    public List<AIComponentGene> genes_s = new();

    public GeneDataRecord Clone()
    {
        return new GeneDataRecord
        {
            instanceId = instanceId,
            objectName = objectName,
            species = species,
            phase = phase,
            speciesID = speciesID,
            fromCheckpoint = fromCheckpoint,
            checkpointReason = checkpointReason,
            checkpointTime = checkpointTime,
            valueGene = valueGene != null ? valueGene.Clone() : null,
            genes_s = genes_s != null ? new List<AIComponentGene>(genes_s) : new List<AIComponentGene>()
        };
    }
}
