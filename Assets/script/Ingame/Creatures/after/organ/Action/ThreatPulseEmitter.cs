using UnityEngine;

public class ThreatPulseEmitter : MonoBehaviour, IAIAction
{
    public float pulseScore = 3f;
    public float pulseRadius = 3f;
    public float interval = 0.5f;
    float nextPulseTime;
    threatmap_calc cachedThreatMap;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.Transform == null || Time.time < nextPulseTime)
            return false;

        nextPulseTime = Time.time + Mathf.Max(0.02f, interval);
        threatmap_calc threatMap = GetThreatMap();
        if (threatMap == null)
            return false;

        threatMap.AddThreatPulse(context.Transform.position, pulseScore, pulseRadius);
        return true;
    }

    public bool EmitAttackPulse(Vector3 point)
    {
        threatmap_calc threatMap = GetThreatMap();
        if (threatMap == null)
            return false;

        threatMap.AddThreatPulse(point, pulseScore, pulseRadius);
        return true;
    }

    threatmap_calc GetThreatMap()
    {
        if (cachedThreatMap == null)
            cachedThreatMap = FindFirstObjectByType<threatmap_calc>();
        return cachedThreatMap;
    }
}
