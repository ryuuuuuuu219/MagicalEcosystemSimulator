using UnityEngine;

public class PredatorVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject nearestPredator;

    public void TickSense(AIContext context, float deltaTime)
    {
        nearestPredator = null;
    }
}
