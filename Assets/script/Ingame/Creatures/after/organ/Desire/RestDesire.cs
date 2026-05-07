public class RestDesire : UnityEngine.MonoBehaviour, IAIDesire
{
    public AIMoveIntent Evaluate(AIContext context)
    {
        return AIMoveIntent.None("rest not implemented");
    }
}
