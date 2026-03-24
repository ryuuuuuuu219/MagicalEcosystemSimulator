using UnityEngine;

public static class CreatureBalanceTuning
{
    public const float HerbivoreMaxHealth = 35f;
    public const float PredatorMaxHealth = 55f;

    const float LegacyAttackDamageMin = 3f;
    const float LegacyAttackDamageMax = 7f;
    const float LegacyAttackCooldownMin = 0.7f;
    const float LegacyAttackCooldownMax = 1.5f;

    const float ChargeDamageScaleMin = 0.6f;
    const float ChargeDamageScaleMax = 1.2f;
    const float BiteDamageMin = 4f;
    const float BiteDamageMax = 8f;
    const float MeleeDamageMin = 6f;
    const float MeleeDamageMax = 12f;
    const float AttackClockMin = 0.9f;
    const float AttackClockMax = 1.8f;

    public static void ApplyPredatorCombatDefaults(ref PredatorGenome genome, System.Random rand)
    {
        genome.attackDamage = RandomRange(rand, LegacyAttackDamageMin, LegacyAttackDamageMax);
        genome.attackCooldown = RandomRange(rand, LegacyAttackCooldownMin, LegacyAttackCooldownMax);
        genome.chargeDamageScale = RandomRange(rand, ChargeDamageScaleMin, ChargeDamageScaleMax);
        genome.chargeAttackClock = RandomRange(rand, AttackClockMin, AttackClockMax);
        genome.biteDamage = RandomRange(rand, BiteDamageMin, BiteDamageMax);
        genome.biteAttackClock = RandomRange(rand, AttackClockMin, AttackClockMax);
        genome.meleeDamage = RandomRange(rand, MeleeDamageMin, MeleeDamageMax);
        genome.meleeAttackClock = RandomRange(rand, AttackClockMin, AttackClockMax);
    }

    public static PredatorGenome NormalizePredatorGenome(PredatorGenome genome)
    {
        genome.attackDamage = Mathf.Clamp(genome.attackDamage, LegacyAttackDamageMin, LegacyAttackDamageMax);
        genome.attackCooldown = Mathf.Clamp(genome.attackCooldown, LegacyAttackCooldownMin, LegacyAttackCooldownMax);
        genome.chargeDamageScale = Mathf.Clamp(genome.chargeDamageScale, ChargeDamageScaleMin, ChargeDamageScaleMax);
        genome.chargeAttackClock = Mathf.Clamp(genome.chargeAttackClock, AttackClockMin, AttackClockMax);
        genome.biteDamage = Mathf.Clamp(genome.biteDamage, BiteDamageMin, BiteDamageMax);
        genome.biteAttackClock = Mathf.Clamp(genome.biteAttackClock, AttackClockMin, AttackClockMax);
        genome.meleeDamage = Mathf.Clamp(genome.meleeDamage, MeleeDamageMin, MeleeDamageMax);
        genome.meleeAttackClock = Mathf.Clamp(genome.meleeAttackClock, AttackClockMin, AttackClockMax);
        return genome;
    }

    static float RandomRange(System.Random rand, float min, float max)
    {
        return min + (float)rand.NextDouble() * (max - min);
    }
}
