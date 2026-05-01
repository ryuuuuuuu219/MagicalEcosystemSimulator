using System.Collections.Generic;
using UnityEngine;

public static class LightningImpactEffect
{
    const float MinSegmentLength = 0.45f;
    const float SpeedSegmentTime = 0.025f;
    const float JitterScale = 0.65f;

    public static GameObject Create(Vector3 launchPoint, Vector3 impactPoint, Vector3 normal, float radius, float projectileSpeed, float lifetime)
    {
        GameObject root = new GameObject("Lightning Impact");
        float safeRadius = Mathf.Max(0.2f, radius);
        float safeLifetime = Mathf.Max(0.05f, lifetime);
        Vector3 impactNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;

        Material material = CreateLightningMaterial();
        List<List<Vector3>> pointPaths = new List<List<Vector3>>();
        List<float> widths = new List<float>();
        pointPaths.Add(BuildJitteredPath(launchPoint, impactPoint, safeRadius * 0.22f, projectileSpeed));
        widths.Add(0.075f);

        int branchCount = Mathf.Clamp(Mathf.RoundToInt(safeRadius * 3f), 4, 10);
        Vector3 tangentA = Vector3.Cross(impactNormal, Vector3.up);
        if (tangentA.sqrMagnitude < 0.001f)
            tangentA = Vector3.Cross(impactNormal, Vector3.right);
        tangentA.Normalize();
        Vector3 tangentB = Vector3.Cross(impactNormal, tangentA).normalized;

        for (int i = 0; i < branchCount; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            float distance = Random.Range(safeRadius * 0.35f, safeRadius);
            Vector3 radial = tangentA * Mathf.Cos(angle) + tangentB * Mathf.Sin(angle);
            Vector3 end = impactPoint + radial * distance + impactNormal * Random.Range(0.05f, safeRadius * 0.3f);
            float width = Mathf.Lerp(0.05f, 0.025f, i / Mathf.Max(1f, branchCount - 1f));
            pointPaths.Add(BuildJitteredPath(impactPoint, end, safeRadius * 0.18f, projectileSpeed));
            widths.Add(width);
        }

        for (int i = 0; i < pointPaths.Count; i++)
        {
            string name = i == 0 ? "Lightning Main Bolt" : $"Lightning Branch {i - 1:00}";
            DrawBolt(root.transform, name, pointPaths[i], material, widths[i]);
        }

        Object.Destroy(root, safeLifetime);
        return root;
    }

    static void DrawBolt(Transform parent, string name, List<Vector3> points, Material material, float width)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        LineRenderer line = obj.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.material = material;
        line.textureMode = LineTextureMode.Stretch;
        line.alignment = LineAlignment.View;
        line.numCapVertices = 3;
        line.numCornerVertices = 2;
        line.startWidth = width;
        line.endWidth = width * 0.35f;
        line.positionCount = 0;

        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    static List<Vector3> BuildJitteredPath(Vector3 start, Vector3 end, float jitterRadius, float projectileSpeed)
    {
        float distance = Vector3.Distance(start, end);
        float segmentLength = Mathf.Max(MinSegmentLength, Mathf.Max(1f, projectileSpeed) * SpeedSegmentTime);
        int segmentCount = Mathf.Clamp(Mathf.CeilToInt(distance / segmentLength), 2, 48);
        float interval = distance / segmentCount;
        float jitter = Mathf.Max(jitterRadius, interval * 1.15f) * JitterScale;

        Vector3 direction = (end - start).sqrMagnitude > 0.001f ? (end - start).normalized : Vector3.forward;
        Vector3 sideA = Vector3.Cross(direction, Vector3.up);
        if (sideA.sqrMagnitude < 0.001f)
            sideA = Vector3.Cross(direction, Vector3.right);
        sideA.Normalize();
        Vector3 sideB = Vector3.Cross(direction, sideA).normalized;

        List<Vector3> points = new List<Vector3>(segmentCount + 1);
        points.Add(start);
        for (int i = 1; i < segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            float spread = Mathf.Sin(t * Mathf.PI);
            Vector3 center = Vector3.Lerp(start, end, t);
            Vector3 along = direction * Random.Range(-interval * 0.25f, interval * 0.25f);
            Vector3 lateral = (sideA * Random.Range(-jitter, jitter) + sideB * Random.Range(-jitter, jitter)) * spread;
            points.Add(center + along + lateral);
        }
        points.Add(end);
        return points;
    }

    static Material CreateLightningMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color color = new Color(0.65f, 0.9f, 1f, 1f);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
