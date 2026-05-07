using UnityEngine;

[System.Serializable]
public struct AIMoveIntent
{
    public Vector3 direction;
    public float weight;
    public string reason;

    public Vector3 Vector => direction.sqrMagnitude > 0.0001f
        ? direction.normalized * Mathf.Max(0f, weight)
        : Vector3.zero;

    public static AIMoveIntent None(string reason = "")
    {
        return new AIMoveIntent
        {
            direction = Vector3.zero,
            weight = 0f,
            reason = reason
        };
    }
}
