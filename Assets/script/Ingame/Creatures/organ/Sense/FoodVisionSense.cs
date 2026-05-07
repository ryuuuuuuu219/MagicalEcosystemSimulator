using UnityEngine;

public class FoodVisionSense : MonoBehaviour, IAISense
{
    public float level = 1f;
    public GameObject bestFood;

    public void TickSense(AIContext context, float deltaTime)
    {
        bestFood = null;
    }
}
