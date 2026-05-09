using UnityEngine;

public static class GeneDataApplier
{
    public static void Apply(GameObject target, ValueGene gene)
    {
        if (target == null || gene == null)
            return;

        ApplyLegacyGenome(target, gene);
        ApplyOrganValues(target, gene);
    }

    static void ApplyLegacyGenome(GameObject target, ValueGene gene)
    {
        if (target.TryGetComponent<herbivoreBehaviour>(out var herbivore))
            herbivore.genome = gene.ToHerbivoreGenome(herbivore.genome);
        if (target.TryGetComponent<predatorBehaviour>(out var predator))
            predator.genome = gene.ToPredatorGenome(predator.genome);
    }

    static void ApplyOrganValues(GameObject target, ValueGene gene)
    {
        if (target.TryGetComponent<GroundMotor>(out var motor))
        {
            if (gene.GroundMotor.forwardForce > 0f)
                motor.forwardForce = gene.GroundMotor.forwardForce;
            if (gene.GroundMotor.turnForce > 0f)
                motor.turnForce = gene.GroundMotor.turnForce;
        }

        if (target.TryGetComponent<FoodVisionSense>(out var foodVision))
        {
            if (gene.FoodVisionSense.visionDistance > 0f)
                foodVision.fallbackVisionDistance = gene.FoodVisionSense.visionDistance;
            if (gene.FoodVisionSense.visionAngle > 0f)
                foodVision.fallbackVisionAngle = gene.FoodVisionSense.visionAngle;
        }

        if (target.TryGetComponent<PredatorVisionSense>(out var predatorVision))
        {
            if (gene.PredatorVisionSense.threatDetectDistance > 0f)
                predatorVision.fallbackDetectDistance = gene.PredatorVisionSense.threatDetectDistance;
        }

        if (target.TryGetComponent<PreyVisionSense>(out var preyVision))
        {
            if (gene.PreyVisionSense.preyDetectDistance > 0f)
                preyVision.fallbackDetectDistance = gene.PreyVisionSense.preyDetectDistance;
        }

        if (target.TryGetComponent<ThreatVisionSense>(out var threatVision))
        {
            if (gene.ThreatVisionSense.threatDetectDistance > 0f)
                threatVision.fallbackDetectDistance = gene.ThreatVisionSense.threatDetectDistance;
        }

        if (target.TryGetComponent<GrassEatAction>(out var grassEat))
        {
            if (gene.GrassEatAction.eatRadius > 0f)
                grassEat.eatRadius = gene.GrassEatAction.eatRadius;
            if (gene.GrassEatAction.eatSpeed > 0f)
                grassEat.eatSpeed = gene.GrassEatAction.eatSpeed;
        }

        if (target.TryGetComponent<CorpseEatAction>(out var corpseEat))
        {
            if (gene.CorpseEatAction.eatRadius > 0f)
                corpseEat.eatRadius = gene.CorpseEatAction.eatRadius;
            if (gene.CorpseEatAction.eatSpeed > 0f)
                corpseEat.eatSpeed = gene.CorpseEatAction.eatSpeed;
        }

        if (target.TryGetComponent<FoodDesire>(out var foodDesire))
            foodDesire.level = PositiveOrFallback(gene.FoodDesire.level, foodDesire.level);
        if (target.TryGetComponent<PreyChaseDesire>(out var preyChase))
            preyChase.level = PositiveOrFallback(gene.PreyChaseDesire.level, preyChase.level);
        if (target.TryGetComponent<ThreatAvoidanceDesire>(out var threatAvoidance))
            threatAvoidance.level = PositiveOrFallback(gene.ThreatAvoidanceDesire.level, threatAvoidance.level);
        if (target.TryGetComponent<WanderDesire>(out var wander))
            wander.level = PositiveOrFallback(gene.WanderDesire.level, wander.level);

        if (target.TryGetComponent<ThreatPulseEmitter>(out var pulse))
        {
            pulse.pulseScore = PositiveOrFallback(gene.ThreatPulseEmitter.pulseScore, pulse.pulseScore);
            pulse.pulseRadius = PositiveOrFallback(gene.ThreatPulseEmitter.pulseRadius, pulse.pulseRadius);
            pulse.interval = PositiveOrFallback(gene.ThreatPulseEmitter.interval, pulse.interval);
        }

        if (target.TryGetComponent<FieldManaAbsorbAction>(out var absorb))
        {
            absorb.manaAbsorbPerSec = PositiveOrFallback(gene.FieldManaAbsorbAction.manaAbsorbPerSec, absorb.manaAbsorbPerSec);
            absorb.absorbRadius = PositiveOrFallback(gene.FieldManaAbsorbAction.absorbRadius, absorb.absorbRadius);
            absorb.convertFieldAbsorb = gene.FieldManaAbsorbAction.convertFieldAbsorb;
            absorb.logScale = PositiveOrFallback(gene.FieldManaAbsorbAction.logScale, absorb.logScale);
        }

        if (target.TryGetComponent<ManaFieldSense>(out var manaSense))
            manaSense.level = PositiveOrFallback(gene.ManaFieldSense.level, manaSense.level);

        if (target.TryGetComponent<MagicAttackAction>(out var magicAttack))
            magicAttack.manaCost = PositiveOrFallback(gene.Magic.attackManaCost, magicAttack.manaCost);
        if (target.TryGetComponent<MagicProjectileAttackAction>(out var projectileAttack))
            projectileAttack.manaCost = PositiveOrFallback(gene.Magic.projectileManaCost, projectileAttack.manaCost);
        if (target.TryGetComponent<MagicDefenseAction>(out var magicDefense))
            magicDefense.manaCost = PositiveOrFallback(gene.Magic.defenseManaCost, magicDefense.manaCost);
        if (target.TryGetComponent<MagicEvasionAction>(out var magicEvasion))
            magicEvasion.manaCost = PositiveOrFallback(gene.Magic.evasionManaCost, magicEvasion.manaCost);
        if (target.TryGetComponent<MagicCooldownState>(out var cooldown))
            cooldown.cooldown = PositiveOrFallback(gene.Magic.cooldown, cooldown.cooldown);
    }

    static float PositiveOrFallback(float value, float fallback)
    {
        return value > 0f ? value : fallback;
    }
}
