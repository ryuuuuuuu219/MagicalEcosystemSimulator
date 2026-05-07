public class ChargeAttackAction : UnityEngine.MonoBehaviour, IAIAction
{
    PredatorCombatLibrary.CombatState combatState = new PredatorCombatLibrary.CombatState
    {
        lastChargeClockTime = -999f
    };

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (!PredatorOrganCombatUtility.TryBuildSetup(context, this, out var setup))
            return false;

        PredatorCombatLibrary.CombatResult result = PredatorCombatLibrary.TryChargeAttackAction(
            setup.predator.genome,
            setup.combatContext,
            combatState,
            true,
            setup.targetCollider);
        combatState = result.nextState;
        PredatorOrganCombatUtility.ApplyResult(context, this, setup, result);
        return result.performed;
    }
}
