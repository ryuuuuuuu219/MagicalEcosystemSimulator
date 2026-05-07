using UnityEngine;

public class ProportionalNavigationSteering : MonoBehaviour, IAISteering
{
    public float navigationConstant = 2f;

    public Vector3 Steer(AIContext context, Vector3 desiredVector)
    {
        TargetTracker tracker = GetComponent<TargetTracker>();
        if (context == null || context.Transform == null || tracker == null || tracker.target == null || !tracker.hasSample)
            return desiredVector;

        Vector3 lineOfSight = tracker.target.transform.position - context.Transform.position;
        lineOfSight.y = 0f;
        float losSqrMag = lineOfSight.sqrMagnitude;
        if (losSqrMag <= 0.0001f)
            return desiredVector;

        Vector3 relativeVelocity = tracker.velocity - context.CurrentVelocity;
        relativeVelocity.y = 0f;
        float losRate = Vector3.Cross(lineOfSight, relativeVelocity).y / losSqrMag;
        float closingSpeed = Mathf.Max(0f, Vector3.Dot(-relativeVelocity, lineOfSight.normalized));
        Vector3 lateral = Vector3.Cross(Vector3.up, lineOfSight.normalized);
        Vector3 correction = lateral * Mathf.Clamp(losRate * closingSpeed * navigationConstant, -1f, 1f);
        return desiredVector + correction;
    }
}
