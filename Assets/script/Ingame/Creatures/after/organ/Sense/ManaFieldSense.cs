using UnityEngine;

public class ManaFieldSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public float sampledMana;

    public void TickSense(AIContext context, float deltaTime)
    {
        if (context == null || context.Transform == null)
        {
            sampledMana = 0f;
            return;
        }

        sampledMana = ManaFieldManager.GetOrCreate().SampleMana(context.Transform.position);
    }
}
