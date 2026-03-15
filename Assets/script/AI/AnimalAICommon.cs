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

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
            {
                memory.RemoveAt(i);
            }
            else
            {
                memory[i] = entry;
            }
        }
    }

    public static void Remember(List<(GameObject obj, float time)> memory, GameObject obj, float memoryTime)
    {
        int index = memory.FindIndex(x => x.obj == obj);
        if (index >= 0)
        {
            memory[index] = (obj, memoryTime);
        }
        else
        {
            memory.Add((obj, memoryTime));
        }
    }

    public static void BuildObjectCache(List<(GameObject obj, float time)> memory, List<GameObject> cache)
    {
        cache.Clear();
        for (int i = 0; i < memory.Count; i++)
        {
            var obj = memory[i].obj;
            if (obj != null) cache.Add(obj);
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
            var w = waves[i];
            sum += w.amplitude * Mathf.Sin(w.frequency * time + w.phase);
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
        float right = (terrainPos.x + size.x) - position.x;
        float bottom = position.z - terrainPos.z;
        float top = (terrainPos.z + size.z) - position.z;

        float distNearBoundary = Mathf.Min(left, right, bottom, top);
        if (distNearBoundary > margin) return Vector3.zero;

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
        float forwardForce,
        float turnForce,
        float deltaTime)
    {
        MovementTelemetry telemetry = default;
        if (actorTransform == null)
            return telemetry;

        float dt = Mathf.Max(deltaTime, 0.0001f);
        GroundSample ground = SampleGround(terrain, actorTransform.position);
        Vector3 groundNormal = ground.isValid ? ground.normal : Vector3.up;
        total = AdjustMovementVectorForTerrain(terrain, actorTransform.position, total);
        Vector3 planarVelocity = Vector3.ProjectOnPlane(currentVelocity, groundNormal);
        float inputMagnitude = Mathf.Clamp01(total.magnitude);
        telemetry.moveDemand = inputMagnitude;

        if (inputMagnitude <= 0.0001f)
        {
            float previousSpeed = currentSpeed;
            currentSpeed = 0f;
            if (planarVelocity.sqrMagnitude > 0.0001f)
            {
                float brakeDelta = Mathf.Min(planarVelocity.magnitude, forwardForce * dt);
                planarVelocity = Vector3.MoveTowards(planarVelocity, Vector3.zero, brakeDelta);
                telemetry.brakingDemand = brakeDelta;
            }

            telemetry.brakingDemand = Mathf.Max(telemetry.brakingDemand, Mathf.Abs(previousSpeed));
            currentVelocity = planarVelocity;
            MoveOnGround(actorTransform, terrain, planarVelocity * dt);
            AlignToGround(actorTransform, terrain, planarVelocity);
            return telemetry;
        }

        Vector3 desiredDir = total.normalized;
        if (desiredDir.sqrMagnitude <= 0.0001f)
            return telemetry;
        desiredDir.Normalize();
        Vector3 currentForward = ProjectDirectionOntoGround(actorTransform.forward, groundNormal, desiredDir);
        float signedAngle = Vector3.SignedAngle(currentForward, desiredDir, groundNormal);
        float turnPenalty = Mathf.Clamp01(Mathf.Abs(signedAngle) / 120f);
        float targetSpeed = forwardForce * inputMagnitude * Mathf.Lerp(1f, 0.35f, turnPenalty);
        float previousSpeedValue = currentSpeed;
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            forwardForce * dt
        );
        float speedDelta = currentSpeed - previousSpeedValue;
        if (speedDelta >= 0f)
            telemetry.accelerationDemand = speedDelta;
        else
            telemetry.brakingDemand = -speedDelta;

        Vector3 desiredVelocity = desiredDir * currentSpeed;
        currentVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity, forwardForce * dt);

        float maxYawDelta = Mathf.Max(0f, turnForce) * dt;
        float appliedYaw = Mathf.Clamp(signedAngle, -maxYawDelta, maxYawDelta);
        Quaternion yawRotation = Quaternion.AngleAxis(appliedYaw, groundNormal);
        Vector3 rotatedForward = yawRotation * currentForward;
        SetGroundRotation(actorTransform, groundNormal, rotatedForward);
        telemetry.turnDemand = Mathf.Abs(appliedYaw);

        MoveOnGround(actorTransform, terrain, currentVelocity * dt);
        AlignToGround(actorTransform, terrain, currentVelocity);

        return telemetry;
    }

    static void MoveOnGround(Transform actorTransform, Terrain terrain, Vector3 displacement)
    {
        if (actorTransform == null)
            return;

        Vector3 nextPosition = actorTransform.position + displacement;
        actorTransform.position = ClampPositionToTerrain(terrain, nextPosition, actorTransform.position.y);
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

        if (worldPosition.x < terrainOrigin.x || worldPosition.x > maxX ||
            worldPosition.z < terrainOrigin.z || worldPosition.z > maxZ)
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

    public static Vector3 AdjustMovementVectorForTerrain(Terrain terrain, Vector3 position, Vector3 vector)
    {
        GroundSample ground = SampleGround(terrain, position);
        Vector3 normal = ground.isValid ? ground.normal : Vector3.up;
        return ProjectDirectionOntoGround(vector, normal, Vector3.zero);
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

    static void SetGroundRotation(Transform actorTransform, Vector3 groundNormal, Vector3 forward)
    {
        Vector3 safeForward = ProjectDirectionOntoGround(forward, groundNormal, Vector3.forward);
        if (safeForward.sqrMagnitude <= 0.0001f)
            return;

        actorTransform.rotation = Quaternion.LookRotation(safeForward, groundNormal);
    }

    public static void ClampRotation(Transform actorTransform, Terrain terrain)
    {
        if (actorTransform == null)
            return;

        AlignToGround(actorTransform, terrain, actorTransform.forward);
    }

    public static float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
