using UnityEngine;

public class DamageAvoidanceDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;
    public Vector3 lastDamageDirection;
    public float memoryTimer;

    public void RecordDamageDirection(Vector3 fromDirection, float memoryTime)
    {
        lastDamageDirection = fromDirection;
        memoryTimer = Mathf.Max(0f, memoryTime);
    }

    public AIMoveIntent Evaluate(AIContext context)
    {
        memoryTimer -= Time.deltaTime;
        if (memoryTimer <= 0f)
            return AIMoveIntent.None("damage avoid inactive");

        return new AIMoveIntent { direction = -lastDamageDirection, weight = level, reason = "damage avoid" };
    }
}
