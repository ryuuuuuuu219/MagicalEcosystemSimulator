public class MeleeAttackAction : UnityEngine.MonoBehaviour, IAIAction
{
    PredatorCombatLibrary.CombatState combatState = new PredatorCombatLibrary.CombatState
    {
        lastMeleeClockTime = -999f
    };

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (!PredatorOrganCombatUtility.TryBuildSetup(context, this, out var setup))
            return false;

        PredatorCombatLibrary.CombatResult result = PredatorCombatLibrary.TryMeleeAttackAction(
            setup.predator.genome,
            setup.combatContext,
            combatState,
            true);
        combatState = result.nextState;
        PredatorOrganCombatUtility.ApplyResult(context, this, setup, result);
        return result.performed;
    }
}
