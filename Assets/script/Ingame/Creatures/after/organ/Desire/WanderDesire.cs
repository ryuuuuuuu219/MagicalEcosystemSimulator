using UnityEngine;

public class WanderDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;
    public float frequency = 0.7f;
    public float angle = 45f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        if (context == null || context.Transform == null)
            return AIMoveIntent.None("wander missing");

        float wave = Mathf.Sin(Time.time * Mathf.Max(0.01f, frequency));
        Vector3 dir = Quaternion.Euler(0f, wave * angle, 0f) * context.Transform.forward;
        return new AIMoveIntent { direction = dir, weight = level, reason = "wander" };
    }
}
