using System.Collections.Generic;
using UnityEngine;

public static class AnimalAICommon
{
    struct GroundSample
    {
        public bool isValid;
        public float height;
        public Vector3 normal;
    }

    public struct MovementTelemetry
    {
        public float moveDemand;
        public float accelerationDemand;
        public float brakingDemand;
        public float turnDemand;
    }

    public static void PrepareLegacyRigidbody(GameObject obj)
    {
        if (obj == null || !obj.TryGetComponent<Rigidbody>(out var rb))
            return;

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public static void PrepareCorpseRigidbody(GameObject obj, float linearDamping = 4f, float angularDamping = 2f)
    {
        if (obj == null || !obj.TryGetComponent<Rigidbody>(out var rb))
            return;

        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, Mathf.Min(velocity.y, 0f), 0f);
        rb.angularVelocity = Vector3.zero;
        rb.linearDamping = Mathf.Max(rb.linearDamping, linearDamping);
        rb.angularDamping = Mathf.Max(rb.angularDamping, angularDamping);
    }

    public static void TickMemory(List<(GameObject obj, float time)> memory, float deltaTime)
    {
        for (int i = memory.Count - 1; i >= 0; i--)
        {
            var entry = memory[i];
            if (entry.obj == null)
            {
                memory.RemoveAt(i);
                continue;
            }

            entry.time -= deltaTime;
            if (entry.time <= 0f)
                memory.RemoveAt(i);
            else
                memory[i] = entry;
        }
    }

    public static void Remember(List<(GameObject obj, float time)> memory, GameObject obj, float memoryTime)
    {
        int index = memory.FindIndex(x => x.obj == obj);
        if (index >= 0)
            memory[index] = (obj, memoryTime);
        else
            memory.Add((obj, memoryTime));
    }

    public static void BuildObjectCache(List<(GameObject obj, float time)> memory, List<GameObject> cache)
    {
        cache.Clear();
        for (int i = 0; i < memory.Count; i++)
        {
            var obj = memory[i].obj;
            if (obj != null)
                cache.Add(obj);
        }
    }

    public static float ComputeThreatLevel(float distance, float detectDistance)
    {
        if (distance > detectDistance)
            return 0f;

        float safeDetectDistance = Mathf.Max(0.001f, detectDistance);
        float k = 1f / safeDetectDistance;
        return Mathf.Exp(k * (safeDetectDistance - distance));
    }

    public static float SumWaveValue(WaveGene[] waves, float time)
    {
        if (waves == null || waves.Length == 0)
            return 0f;

        float sum = 0f;
        for (int i = 0; i < waves.Length; i++)
        {
            var wave = waves[i];
            sum += wave.amplitude * Mathf.Sin(wave.frequency * time + wave.phase);
        }

        return sum;
    }

    public static Vector3 ComputeBoundaryVector(Terrain terrain, Vector3 position, float marginRatio = 0.15f)
    {
        if (terrain == null || terrain.terrainData == null)
            return Vector3.zero;

        Vector3 terrainPos = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        float margin = Mathf.Min(size.x, size.z) * Mathf.Clamp01(marginRatio);

        float left = position.x - terrainPos.x;
        float right = terrainPos.x + size.x - position.x;
        float bottom = position.z - terrainPos.z;
        float top = terrainPos.z + size.z - position.z;

        float distNearBoundary = Mathf.Min(left, right, bottom, top);
        if (distNearBoundary > margin)
            return Vector3.zero;

        float power = 1f - Mathf.Clamp01(distNearBoundary / Mathf.Max(0.001f, margin));
        Vector2 center = new Vector2(terrainPos.x + size.x * 0.5f, terrainPos.z + size.z * 0.5f);
        Vector2 pos2 = new Vector2(position.x, position.z);
        Vector2 push2 = (center - pos2).normalized * power;

        return new Vector3(push2.x, 0f, push2.y);
    }

    public static MovementTelemetry ApplyMovement(
        Transform actorTransform,
        Terrain terrain,
        Vector3 total,
        ref float currentSpeed,
        ref Vector3 currentVelocity,
        ref Vector3 inertialMoveVector,
        ref Vector3 inertialFacingVector,
        float forwardForce,
        float turnForce,
        float deltaTime,
        float lowSpeedTurnMultiplier = 1.4f,
        float highSpeedTurnMultiplier = 0.35f)
    {
        MovementTelemetry telemetry = default;
        if (actorTransform == null)
            return telemetry;

        float dt = Mathf.Max(deltaTime, 0.0001f);
        GroundSample ground = SampleGround(terrain, actorTransform.position);
        Vector3 groundNormal = ground.isValid ? ground.normal : Vector3.up;
        Vector3 planarVelocity = Vector3.ProjectOnPlane(currentVelocity, groundNormal);
        float inputMagnitude = Mathf.Clamp01(total.magnitude);
        telemetry.moveDemand = inputMagnitude;

        Vector3 currentForward = ProjectDirectionOntoGround(actorTransform.forward, groundNormal, Vector3.forward);
        if (inertialFacingVector.sqrMagnitude <= 0.0001f)
            inertialFacingVector = currentForward;
        else
            inertialFacingVector = ProjectDirectionOntoGround(inertialFacingVector, groundNormal, currentForward);

        float moveBlend = 1f - Mathf.Exp(-Mathf.Max(1f, forwardForce * 0.35f) * dt);

        if (inputMagnitude <= 0.0001f)
        {
            float previousSpeed = currentSpeed;
            currentSpeed = 0f;
            inertialMoveVector = Vector3.Lerp(inertialMoveVector, Vector3.zero, moveBlend);

            if (planarVelocity.sqrMagnitude > 0.0001f)
            {
                Vector3 velocityDir = planarVelocity.normalized;
                float brakingTurnForce = ComputeSpeedScaledTurnForce(
                    turnForce,
                    planarVelocity.magnitude,
                    forwardForce,
                    lowSpeedTurnMultiplier,
                    highSpeedTurnMultiplier);
                inertialFacingVector = RotateTowardOnGround(
                    currentForward,
                    velocityDir,
                    groundNormal,
                    brakingTurnForce,
                    dt,
                    out float brakingYaw);
                SetGroundRotation(actorTransform, groundNormal, inertialFacingVector);
                telemetry.turnDemand = Mathf.Abs(brakingYaw);

                float brakeDelta = Mathf.Min(planarVelocity.magnitude, forwardForce * dt);
                planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, brakeDelta);
                telemetry.brakingDemand = brakeDelta;
            }

            telemetry.brakingDemand = Mathf.Max(telemetry.brakingDemand, Mathf.Abs(previousSpeed));
            currentVelocity = planarVelocity;
            MoveOnGround(actorTransform, terrain, planarVelocity * dt);
            return telemetry;
        }

        Vector3 desiredDir = total.normalized;
        if (desiredDir.sqrMagnitude <= 0.0001f)
            return telemetry;

        inertialMoveVector = inertialMoveVector.sqrMagnitude <= 0.0001f
            ? desiredDir
            : Vector3.Slerp(inertialMoveVector.normalized, desiredDir, moveBlend).normalized;

        Vector3 smoothedDir = ProjectDirectionOntoGround(inertialMoveVector, groundNormal, desiredDir);
        Vector3 desiredFacing = ProjectDirectionOntoGround(desiredDir, groundNormal, smoothedDir);

        float signedAngle = Vector3.SignedAngle(currentForward, desiredFacing, groundNormal);
        float turnPenalty = Mathf.Clamp01(Mathf.Abs(signedAngle) / 120f);
        float targetSpeed = forwardForce * inputMagnitude * Mathf.Lerp(1f, 0.35f, turnPenalty);

        float previousSpeedValue = currentSpeed;
        float effectiveTurnForce = ComputeSpeedScaledTurnForce(
            turnForce,
            previousSpeedValue,
            forwardForce,
            lowSpeedTurnMultiplier,
            highSpeedTurnMultiplier);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, forwardForce * dt);

        float speedDelta = currentSpeed - previousSpeedValue;
        if (speedDelta >= 0f)
            telemetry.accelerationDemand = speedDelta;
        else
            telemetry.brakingDemand = -speedDelta;

        Vector3 desiredVelocity = smoothedDir * currentSpeed;
        currentVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, forwardForce * dt);

        Vector3 rotatedForward = RotateTowardOnGround(
            currentForward,
            desiredFacing,
            groundNormal,
            effectiveTurnForce,
            dt,
            out float appliedYaw);
        inertialFacingVector = rotatedForward;

        SetGroundRotation(actorTransform, groundNormal, rotatedForward);
        telemetry.turnDemand = Mathf.Abs(appliedYaw);

        MoveOnGround(actorTransform, terrain, currentVelocity * dt);
        return telemetry;
    }

    static float ComputeSpeedScaledTurnForce(
        float turnForce,
        float currentSpeed,
        float topSpeed,
        float lowSpeedTurnMultiplier,
        float highSpeedTurnMultiplier)
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(0.0001f, Mathf.Abs(topSpeed)));
        float turnScale = Mathf.Lerp(
            Mathf.Max(0f, lowSpeedTurnMultiplier),
            Mathf.Max(0f, highSpeedTurnMultiplier),
            speedRatio);
        return Mathf.Max(0f, turnForce) * turnScale;
    }

    static Vector3 RotateTowardOnGround(
        Vector3 currentForward,
        Vector3 desiredForward,
        Vector3 groundNormal,
        float turnForce,
        float dt,
        out float appliedYaw)
    {
        Vector3 safeDesired = ProjectDirectionOntoGround(desiredForward, groundNormal, currentForward);
        float signedAngle = Vector3.SignedAngle(currentForward, safeDesired, groundNormal);
        float maxYawDelta = Mathf.Max(0f, turnForce) * Mathf.Max(dt, 0.0001f);
        appliedYaw = Mathf.Clamp(signedAngle, -maxYawDelta, maxYawDelta);
        return ProjectDirectionOntoGround(
            Quaternion.AngleAxis(appliedYaw, groundNormal) * currentForward,
            groundNormal,
            safeDesired);
    }

    public static Vector3 AdjustMovementVectorForTerrain(Terrain terrain, Vector3 position, Vector3 vector)
    {
        GroundSample ground = SampleGround(terrain, position);
        Vector3 normal = ground.isValid ? ground.normal : Vector3.up;
        return ProjectDirectionOntoGround(vector, normal, Vector3.zero);
    }

    public static void ClampRotation(Transform actorTransform, Terrain terrain)
    {
        if (actorTransform == null)
            return;

        AlignToGround(actorTransform, terrain, GetCurrentForward(actorTransform));
    }

    public static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    static void MoveOnGround(Transform actorTransform, Terrain terrain, Vector3 displacement)
    {
        if (actorTransform == null)
            return;

        Vector3 nextPosition = actorTransform.position + displacement;
        nextPosition.y = actorTransform.position.y;
        nextPosition = ClampPositionToTerrain(terrain, nextPosition, actorTransform.position.y);

        if (actorTransform.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
        {
            Vector3 currentRbVelocity = rb.linearVelocity;
            float safeDt = Mathf.Max(Time.fixedDeltaTime, 0.0001f);
            Vector3 adjustedDisplacement = nextPosition - actorTransform.position;
            Vector3 horizontalVelocity = adjustedDisplacement / safeDt;
            horizontalVelocity.y = currentRbVelocity.y;
            rb.linearVelocity = horizontalVelocity;
            return;
        }

        actorTransform.position = nextPosition;
    }

    static Vector3 ClampPositionToTerrain(Terrain terrain, Vector3 position, float fallbackY, float padding = 0.5f)
    {
        if (terrain == null || terrain.terrainData == null)
            return position;

        Vector3 terrainOrigin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        float safePaddingX = Mathf.Min(Mathf.Max(0f, padding), size.x * 0.5f);
        float safePaddingZ = Mathf.Min(Mathf.Max(0f, padding), size.z * 0.5f);

        position.x = Mathf.Clamp(position.x, terrainOrigin.x + safePaddingX, terrainOrigin.x + size.x - safePaddingX);
        position.z = Mathf.Clamp(position.z, terrainOrigin.z + safePaddingZ, terrainOrigin.z + size.z - safePaddingZ);

        GroundSample ground = SampleGround(terrain, position);
        position.y = ground.isValid ? ground.height : fallbackY;
        return position;
    }

    static GroundSample SampleGround(Terrain terrain, Vector3 worldPosition)
    {
        GroundSample sample = default;
        if (terrain == null || terrain.terrainData == null)
            return sample;

        Vector3 terrainOrigin = terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        float maxX = terrainOrigin.x + size.x;
        float maxZ = terrainOrigin.z + size.z;

        if (worldPosition.x < terrainOrigin.x ||
            worldPosition.x > maxX ||
            worldPosition.z < terrainOrigin.z ||
            worldPosition.z > maxZ)
        {
            return sample;
        }

        float normalizedX = Mathf.InverseLerp(terrainOrigin.x, maxX, worldPosition.x);
        float normalizedZ = Mathf.InverseLerp(terrainOrigin.z, maxZ, worldPosition.z);

        sample.isValid = true;
        sample.height = terrain.SampleHeight(worldPosition) + terrainOrigin.y;
        sample.normal = terrain.terrainData.GetInterpolatedNormal(normalizedX, normalizedZ).normalized;
        if (sample.normal.sqrMagnitude <= 0.0001f)
            sample.normal = Vector3.up;

        return sample;
    }

    static Vector3 ProjectDirectionOntoGround(Vector3 vector, Vector3 groundNormal, Vector3 fallback)
    {
        Vector3 projected = Vector3.ProjectOnPlane(vector, groundNormal);
        if (projected.sqrMagnitude <= 0.0001f)
            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.zero;

        return projected.normalized;
    }

    static void AlignToGround(Transform actorTransform, Terrain terrain, Vector3 desiredForward)
    {
        GroundSample ground = SampleGround(terrain, actorTransform.position);
        Vector3 normal = ground.isValid ? ground.normal : Vector3.up;
        Vector3 forward = ProjectDirectionOntoGround(desiredForward, normal, actorTransform.forward);
        if (forward.sqrMagnitude <= 0.0001f)
            forward = ProjectDirectionOntoGround(actorTransform.forward, normal, Vector3.forward);

        SetGroundRotation(actorTransform, normal, forward);
    }

    static Vector3 GetCurrentForward(Transform actorTransform)
    {
        if (actorTransform != null &&
            actorTransform.TryGetComponent<Rigidbody>(out var rb) &&
            !rb.isKinematic)
        {
            return rb.rotation * Vector3.forward;
        }

        return actorTransform != null ? actorTransform.forward : Vector3.forward;
    }

    static void SetGroundRotation(Transform actorTransform, Vector3 groundNormal, Vector3 forward)
    {
        Vector3 safeForward = ProjectDirectionOntoGround(forward, groundNormal, Vector3.forward);
        if (safeForward.sqrMagnitude <= 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(safeForward, groundNormal);
        if (actorTransform.TryGetComponent<Rigidbody>(out var rb) && !rb.isKinematic)
        {
            rb.MoveRotation(targetRotation);
            return;
        }

        actorTransform.rotation = targetRotation;
    }
}
