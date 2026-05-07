using UnityEngine;

public static class PredatorCombatLibrary
{
    public struct CombatState
    {
        public float lastChargeClockTime;
        public float lastBiteClockTime;
        public float lastMeleeClockTime;
    }

    public struct CombatResult
    {
        public bool performed;
        public float damage;
        public float manaCost;
        public bool copyTargetVelocity;
        public Vector3 inheritedVelocity;
        public Vector3 inheritedMoveDirection;
        public CombatState nextState;
    }

    public struct CombatContext
    {
        public Transform attacker;
        public Collider attackerCollider;
        public Vector3 attackerVelocity;
        public Vector3 targetPosition;
        public Vector3 targetVelocity;
        public Vector3 targetForward;
        public float currentTime;
    }

    public static CombatResult TryCombatActions(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        bool canAttackLivePrey,
        Collider targetCollider)
    {
        CombatResult result = new CombatResult { nextState = state };

        if (!canAttackLivePrey || context.attacker == null || targetCollider == null)
            return result;

        if (TryChargeAttack(genome, context, result.nextState, targetCollider, out result))
            return result;
        if (TryBiteAttack(genome, context, result.nextState, out result))
            return result;
        if (TryMeleeAttack(genome, context, result.nextState, out result))
            return result;
        return result;
    }

    public static CombatResult TryChargeAttackAction(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        bool canAttackLivePrey,
        Collider targetCollider)
    {
        CombatResult result = new CombatResult { nextState = state };
        if (!canAttackLivePrey || context.attacker == null || targetCollider == null)
            return result;

        TryChargeAttack(genome, context, state, targetCollider, out result);
        return result;
    }

    public static CombatResult TryBiteAttackAction(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        bool canAttackLivePrey)
    {
        CombatResult result = new CombatResult { nextState = state };
        if (!canAttackLivePrey || context.attacker == null)
            return result;

        TryBiteAttack(genome, context, state, out result);
        return result;
    }

    public static CombatResult TryMeleeAttackAction(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        bool canAttackLivePrey)
    {
        CombatResult result = new CombatResult { nextState = state };
        if (!canAttackLivePrey || context.attacker == null)
            return result;

        TryMeleeAttack(genome, context, state, out result);
        return result;
    }

    static bool TryChargeAttack(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        Collider targetCollider,
        out CombatResult result)
    {
        result = new CombatResult { nextState = state };
        float clock = Mathf.Max(0.8f, genome.chargeAttackClock);
        if (context.currentTime < state.lastChargeClockTime + clock)
            return false;
        if (!IsTargetInsideFrontQuadrant(context.attacker, context.targetPosition))
            return false;
        if (!IsTargetInsideAttackArc(context.attacker, context.targetPosition, genome.chargeArc))
            return false;
        if (!IsColliderContact(context.attackerCollider, targetCollider, Mathf.Max(0.01f, genome.chargeContactPadding)))
            return false;

        state.lastChargeClockTime = context.currentTime;
        float probability = Mathf.Clamp(80f - 2f * ComputeWorldDistance(context), 0f, 100f);
        if (!RollProbability(probability))
        {
            result.nextState = state;
            return false;
        }

        float damageCoefficient = ComputeAttackPowerCoefficient(clock);
        float relativeSpeed = ComputeRelativeSpeed(context.attackerVelocity, context.targetVelocity);
        float damage = Mathf.Max(0f, genome.chargeDamageScale) * relativeSpeed * damageCoefficient;
        if (damage <= 0.001f)
        {
            result.nextState = state;
            return false;
        }

        Vector3 contactPoint = ComputeContactPoint(context.attackerCollider, targetCollider, context.targetPosition);
        AttackTraceLibrary.DrawChargeBurst(
            contactPoint,
            context.attacker.position,
            genome.attackTraceScale,
            genome.attackTraceDuration,
            genome.attackTraceDepth);
        result = new CombatResult
        {
            performed = true,
            damage = damage,
            manaCost = Mathf.Max(0f, genome.chargeManaCost),
            nextState = state
        };
        return true;
    }

    static bool TryBiteAttack(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        out CombatResult result)
    {
        result = new CombatResult { nextState = state };
        float clock = Mathf.Max(0.8f, genome.biteAttackClock);
        if (context.currentTime < state.lastBiteClockTime + clock)
            return false;
        if (!IsTargetInsideFrontQuadrant(context.attacker, context.targetPosition))
            return false;
        if (!IsTargetInsideAttackArc(context.attacker, context.targetPosition, genome.biteArc))
            return false;

        state.lastBiteClockTime = context.currentTime;
        float probability = Mathf.Clamp(60f - ComputeWorldDistance(context), 0f, 100f);
        if (!RollProbability(probability))
        {
            result.nextState = state;
            return false;
        }

        float damage = Mathf.Max(0f, genome.biteDamage) * ComputeAttackPowerCoefficient(clock);
        AttackTraceLibrary.DrawBiteProjection(
            context.targetPosition,
            genome.attackTraceScale,
            genome.attackTraceDuration,
            genome.attackTraceDepth);
        result = new CombatResult
        {
            performed = true,
            damage = damage,
            manaCost = Mathf.Max(0f, genome.biteManaCost),
            copyTargetVelocity = true,
            inheritedVelocity = Flatten(context.targetVelocity),
            inheritedMoveDirection = ComputeInheritedMoveDirection(context.targetVelocity, context.targetForward),
            nextState = state
        };
        return true;
    }

    static bool TryMeleeAttack(
        in PredatorGenome genome,
        in CombatContext context,
        CombatState state,
        out CombatResult result)
    {
        result = new CombatResult { nextState = state };
        float clock = Mathf.Max(0.8f, genome.meleeAttackClock);
        if (context.currentTime < state.lastMeleeClockTime + clock)
            return false;
        if (!IsTargetInsideFrontQuadrant(context.attacker, context.targetPosition))
            return false;
        if (!IsTargetInsideAttackArc(context.attacker, context.targetPosition, genome.meleeArc))
            return false;

        state.lastMeleeClockTime = context.currentTime;
        float distance = ComputeWorldDistance(context);
        float probability = Mathf.Clamp((-0.8f * distance * distance) + 70f + (7f * distance), 0f, 100f);
        if (!RollProbability(probability))
        {
            result.nextState = state;
            return false;
        }

        float damage = Mathf.Max(0f, genome.meleeDamage) * ComputeAttackPowerCoefficient(clock);
        AttackTraceLibrary.DrawMeleeArc(
            context.targetPosition,
            context.attacker.forward,
            genome.meleeArc.radius + genome.meleeArc.length,
            genome.meleeArc.arcDegrees,
            genome.attackTraceScale,
            genome.attackTraceDuration,
            genome.attackTraceDepth);
        result = new CombatResult
        {
            performed = true,
            damage = damage,
            manaCost = Mathf.Max(0f, genome.meleeManaCost),
            nextState = state
        };
        return true;
    }

    static bool IsTargetInsideFrontQuadrant(Transform attacker, Vector3 targetPosition)
    {
        Vector3 local = attacker.InverseTransformPoint(targetPosition);
        local.y = 0f;
        if (local.z <= 0f)
            return false;

        return Mathf.Abs(local.x) <= local.z;
    }

    static bool IsTargetInsideAttackArc(Transform attacker, Vector3 targetPosition, AttackArcSettings settings)
    {
        Vector3 localTarget = attacker.InverseTransformPoint(targetPosition) - settings.startOffset;
        localTarget.y = 0f;

        float dist = localTarget.magnitude;
        float innerRadius = Mathf.Max(0f, settings.radius);
        float outerRadius = innerRadius + Mathf.Max(0f, settings.length);
        if (dist < innerRadius || dist > outerRadius)
            return false;

        Vector3 localDir = settings.localDirection.sqrMagnitude > 0.0001f
            ? settings.localDirection.normalized
            : Vector3.forward;
        localDir.y = 0f;
        if (localDir.sqrMagnitude <= 0.0001f)
            localDir = Vector3.forward;

        float angle = Vector3.Angle(localDir, localTarget.normalized);
        return angle <= Mathf.Max(0.1f, settings.arcDegrees) * 0.5f;
    }

    static bool IsColliderContact(Collider attackerCollider, Collider targetCollider, float padding)
    {
        if (attackerCollider == null || targetCollider == null)
            return false;

        Vector3 closestSelf = attackerCollider.ClosestPoint(targetCollider.bounds.center);
        Vector3 closestTarget = targetCollider.ClosestPoint(attackerCollider.bounds.center);
        float dist = Vector3.Distance(closestSelf, closestTarget);
        return dist <= padding;
    }

    static float ComputeRelativeSpeed(Vector3 attackerVelocity, Vector3 targetVelocity)
    {
        return (Flatten(attackerVelocity) - Flatten(targetVelocity)).magnitude;
    }

    static float ComputeWorldDistance(in CombatContext context)
    {
        return Vector3.Distance(context.attacker.position, context.targetPosition);
    }

    static float ComputeAttackPowerCoefficient(float clock)
    {
        return Mathf.Max(0.8f, clock) / 0.8f;
    }

    static bool RollProbability(float probabilityPercent)
    {
        return Random.value * 100f <= Mathf.Clamp(probabilityPercent, 0f, 100f);
    }

    static Vector3 ComputeContactPoint(Collider attackerCollider, Collider targetCollider, Vector3 fallback)
    {
        if (attackerCollider == null || targetCollider == null)
            return fallback;

        Vector3 a = attackerCollider.ClosestPoint(targetCollider.bounds.center);
        Vector3 b = targetCollider.ClosestPoint(attackerCollider.bounds.center);
        return (a + b) * 0.5f;
    }

    static Vector3 ComputeInheritedMoveDirection(Vector3 targetVelocity, Vector3 targetForward)
    {
        Vector3 flattenedVelocity = Flatten(targetVelocity);
        if (flattenedVelocity.sqrMagnitude > 0.0001f)
            return flattenedVelocity.normalized;

        Vector3 flattenedForward = Flatten(targetForward);
        return flattenedForward.sqrMagnitude > 0.0001f ? flattenedForward.normalized : Vector3.forward;
    }

    static Vector3 Flatten(Vector3 vector)
    {
        vector.y = 0f;
        return vector;
    }

}
