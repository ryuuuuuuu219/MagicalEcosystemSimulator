using UnityEngine;

public class SafeFoodPathEvaluator : MonoBehaviour
{
    public bool IsSafe(Vector3 selfPosition, Vector3 foodPosition)
    {
        return Vector3.Distance(selfPosition, foodPosition) > 0.01f;
    }
}
