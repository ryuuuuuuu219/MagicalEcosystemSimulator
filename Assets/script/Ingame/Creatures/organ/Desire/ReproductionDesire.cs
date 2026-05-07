public class ReproductionDesire : UnityEngine.MonoBehaviour, IAIDesire
{
    public AIMoveIntent Evaluate(AIContext context)
    {
        return AIMoveIntent.None("reproduction not implemented");
    }
}
