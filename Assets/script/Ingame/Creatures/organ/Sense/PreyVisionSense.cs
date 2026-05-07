using UnityEngine;

public class PreyVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject bestPrey;

    public void TickSense(AIContext context, float deltaTime)
    {
        bestPrey = null;
    }
}
