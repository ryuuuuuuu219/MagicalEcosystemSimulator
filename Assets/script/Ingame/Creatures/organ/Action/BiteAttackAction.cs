public class BiteAttackAction : UnityEngine.MonoBehaviour, IAIAction
{
    PredatorCombatLibrary.CombatState combatState = new PredatorCombatLibrary.CombatState
    {
        lastBiteClockTime = -999f
    };

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (!PredatorOrganCombatUtility.TryBuildSetup(context, this, out var setup))
            return false;

        PredatorCombatLibrary.CombatResult result = PredatorCombatLibrary.TryBiteAttackAction(
            setup.predator.genome,
            setup.combatContext,
            combatState,
            true);
        combatState = result.nextState;
        PredatorOrganCombatUtility.ApplyResult(context, this, setup, result);
        return result.performed;
    }
}
