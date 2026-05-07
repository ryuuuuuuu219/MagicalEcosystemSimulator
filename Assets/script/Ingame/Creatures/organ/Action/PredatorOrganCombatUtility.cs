using UnityEngine;

public static class PredatorOrganCombatUtility
{
    public struct AttackSetup
    {
        public predatorBehaviour predator;
        public GameObject target;
        public Collider selfCollider;
        public Collider targetCollider;
        public PredatorCombatLibrary.CombatContext combatContext;
    }

    public static bool TryBuildSetup(AIContext context, MonoBehaviour owner, out AttackSetup setup)
    {
        setup = default;
        if (context == null || context.Transform == null || owner == null)
            return false;
        if (!owner.TryGetComponent<predatorBehaviour>(out var predator))
            return false;
        if (predator.IsDead || !CanAttackLivePrey(context))
            return false;
        if (!owner.TryGetComponent<PreyMemory>(out var preyMemory) || preyMemory.rememberedPrey == null)
            return false;
        if (IsPreyDead(preyMemory.rememberedPrey))
            return false;
        if (!owner.TryGetComponent<Collider>(out var selfCollider))
            return false;
        if (!preyMemory.rememberedPrey.TryGetComponent<Collider>(out var targetCollider))
            return false;

        Vector3 targetVelocity = GetTargetVelocity(preyMemory.rememberedPrey);
        setup = new AttackSetup
        {
            predator = predator,
            target = preyMemory.rememberedPrey,
            selfCollider = selfCollider,
            targetCollider = targetCollider,
            combatContext = new PredatorCombatLibrary.CombatContext
            {
                attacker = owner.transform,
                attackerCollider = selfCollider,
                attackerVelocity = context.CurrentVelocity,
                targetPosition = preyMemory.rememberedPrey.transform.position,
                targetVelocity = targetVelocity,
                targetForward = preyMemory.rememberedPrey.transform.forward,
                currentTime = Time.time
            }
        };
        return true;
    }

    public static void ApplyResult(AIContext context, MonoBehaviour owner, AttackSetup setup, PredatorCombatLibrary.CombatResult result)
    {
        if (!result.performed || setup.target == null)
            return;

        ApplyDamageToTarget(setup.target, result.damage);
        if (context != null && context.BodyResource != null)
        {
            context.BodyResource.AddMana(result.damage, out _, "organ attack drain");
            context.BodyResource.RemoveMana(result.manaCost, "organ attack cost");
        }

        if (owner != null && owner.TryGetComponent<ThreatPulseEmitter>(out var pulse))
            pulse.EmitAttackPulse(setup.target.transform.position);

        if (result.copyTargetVelocity && owner != null && owner.TryGetComponent<GroundMotor>(out var motor))
            motor.InheritVelocity(result.inheritedVelocity);
    }

    static bool CanAttackLivePrey(AIContext context)
    {
        if (context == null || context.MaxMana <= 0f)
            return true;

        return context.Mana / context.MaxMana > 0.1f;
    }

    static bool IsPreyDead(GameObject prey)
    {
        if (prey == null)
            return true;
        if (prey.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return herbivore.IsDead;
        if (prey.TryGetComponent<predatorBehaviour>(out var predator))
            return predator.IsDead;
        return false;
    }

    static Vector3 GetTargetVelocity(GameObject target)
    {
        if (target == null)
            return Vector3.zero;
        if (target.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return herbivore.CurrentVelocity;
        if (target.TryGetComponent<predatorBehaviour>(out var predator))
            return predator.CurrentVelocity;
        if (target.TryGetComponent<GroundMotor>(out var motor))
            return motor.CurrentVelocity;
        return Vector3.zero;
    }

    static void ApplyDamageToTarget(GameObject target, float damage)
    {
        if (target == null || damage <= 0f)
            return;
        if (target.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            herbivore.TakeDamage(damage);
            return;
        }
        if (target.TryGetComponent<predatorBehaviour>(out var predator))
            predator.TakeDamage(damage);
    }
}
