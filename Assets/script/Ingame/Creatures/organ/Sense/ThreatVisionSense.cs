using UnityEngine;

public class ThreatVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject nearestThreat;

    public void TickSense(AIContext context, float deltaTime)
    {
        nearestThreat = null;
    }
}
