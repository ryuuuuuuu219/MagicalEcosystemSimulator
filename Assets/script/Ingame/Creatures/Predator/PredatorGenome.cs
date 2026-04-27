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

