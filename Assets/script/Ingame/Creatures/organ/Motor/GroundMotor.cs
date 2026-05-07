using UnityEngine;

public class GroundMotor : MonoBehaviour
{
    public float forwardForce = 8f;
    public float turnForce = 180f;
    public MovementTelemetry lastTelemetry;
    float currentSpeed;
    Vector3 currentVelocity;
    Vector3 inertialMoveVector;
    Vector3 inertialFacingVector;

    public Vector3 CurrentVelocity => currentVelocity;

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
            deltaTime);

        lastTelemetry = MovementTelemetry.From(telemetry);
    }
}
