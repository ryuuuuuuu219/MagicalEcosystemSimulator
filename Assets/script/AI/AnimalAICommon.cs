using System.Collections.Generic;
using UnityEngine;

public static class AnimalAICommon
{
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

    public static void ApplyMovement(Rigidbody rb, Vector3 total, ref float currentSpeed, float forwardForce, float turnForce)
    {
        if (total.sqrMagnitude < 0.0001f)
        {
            currentSpeed = 0f;
            Vector3 currentVel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, currentVel.y, 0f);
            return;
        }

        float targetSpeed = forwardForce;
        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            forwardForce * Time.deltaTime
        );

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = rb.transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);

        Vector3 desiredDir = total.normalized;
        Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
        targetRot = Quaternion.Euler(0f, targetRot.eulerAngles.y, 0f);

        rb.MoveRotation(
            Quaternion.RotateTowards(
                rb.rotation,
                targetRot,
                turnForce * Time.deltaTime
            )
        );
    }

    public static void ClampRotation(Rigidbody rb, float maxTilt = 45f, float torqueScale = 5f)
    {
        Vector3 euler = rb.rotation.eulerAngles;

        float pitch = NormalizeAngle(euler.x);
        float roll = NormalizeAngle(euler.z);

        float pitchError = 0f;
        float rollError = 0f;

        if (pitch > maxTilt) pitchError = pitch - maxTilt;
        if (pitch < -maxTilt) pitchError = pitch + maxTilt;

        if (roll > maxTilt) rollError = roll - maxTilt;
        if (roll < -maxTilt) rollError = roll + maxTilt;

        Vector3 correctionTorque = new Vector3(-pitchError, 0f, -rollError) * torqueScale;
        rb.AddTorque(correctionTorque, ForceMode.Acceleration);
    }

    public static float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
