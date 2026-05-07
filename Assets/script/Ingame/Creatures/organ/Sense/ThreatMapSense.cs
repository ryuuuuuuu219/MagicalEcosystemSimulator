using UnityEngine;

public class ThreatMapSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public float sampledThreat;

    public void TickSense(AIContext context, float deltaTime)
    {
        sampledThreat = 0f;
    }
}
