using UnityEngine;

public class GroundMotor : MonoBehaviour
{
    public float forwardForce = 8f;
    public float turnForce = 180f;
    public float lowSpeedTurnMultiplier = 1.4f;
    public float highSpeedTurnMultiplier = 0.35f;
    public MovementTelemetry lastTelemetry;
    float currentSpeed;
    Vector3 currentVelocity;
    Vector3 inertialMoveVector;
    Vector3 inertialFacingVector;

    public Vector3 CurrentVelocity => currentVelocity;

    public void InheritVelocity(Vector3 velocity)
    {
        currentVelocity = velocity;
        currentVelocity.y = 0f;
        currentSpeed = currentVelocity.magnitude;
        if (currentVelocity.sqrMagnitude > 0.0001f)
        {
            inertialMoveVector = currentVelocity.normalized;
            inertialFacingVector = currentVelocity.normalized;
        }
    }

    public void Stop()
    {
        currentSpeed = 0f;
        currentVelocity = Vector3.zero;
        inertialMoveVector = Vector3.zero;
        inertialFacingVector = transform != null ? transform.forward : Vector3.forward;
        lastTelemetry = default;
    }

    public void Move(AIContext context, Vector3 moveVector, float deltaTime)
    {
        if (context == null || context.Transform == null)
            return;

        AnimalAICommon.MovementTelemetry telemetry = AnimalAICommon.ApplyMovement(
            context.Transform,
            context.Terrain,
            moveVector,
            ref currentSpeed,
            ref currentVelocity,
            ref inertialMoveVector,
            ref inertialFacingVector,
            forwardForce,
            turnForce,
            deltaTime,
            lowSpeedTurnMultiplier,
            highSpeedTurnMultiplier);

        lastTelemetry = MovementTelemetry.From(telemetry);
    }
}
