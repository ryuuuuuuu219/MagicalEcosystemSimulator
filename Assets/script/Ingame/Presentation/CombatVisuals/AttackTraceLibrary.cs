using System.Collections.Generic;
using UnityEngine;

public static class AttackTraceLibrary
{
    sealed class AttackTraceHost : MonoBehaviour
    {
        readonly List<ActiveTrace> activeTraces = new();
        Material sharedMaterial;

        void Update()
        {
            float now = Time.time;
            for (int i = activeTraces.Count - 1; i >= 0; i--)
            {
                if (!activeTraces[i].Tick(now))
                {
                    activeTraces[i].Dispose();
                    activeTraces.RemoveAt(i);
                }
            }
        }

        public void DrawSegments(Camera cam, IReadOnlyList<Vector3> starts, IReadOnlyList<Vector3> ends, float cameraOffset, float duration)
        {
            if (cam == null || starts == null || ends == null)
                return;

            int count = Mathf.Min(starts.Count, ends.Count);
            if (count <= 0)
                return;

            EnsureMaterial();
            float safeDuration = Mathf.Max(0.05f, duration);
            float safeOffset = Mathf.Max(0f, cameraOffset);
            float safeWidthPixels = ResolveWidthPixels();

            for (int i = 0; i < count; i++)
            {
                GameObject lineObj = new GameObject("AttackTraceLine");
                lineObj.transform.SetParent(transform, false);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.useWorldSpace = true;
                lr.alignment = LineAlignment.View;
                lr.textureMode = LineTextureMode.Stretch;
                lr.numCapVertices = 4;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                lr.material = sharedMaterial;
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                lr.widthMultiplier = 1f;
                lr.sortingOrder = short.MaxValue;
                Vector3 cameraPush = cam.transform.forward * safeOffset;
                Vector3 start = starts[i] + cameraPush;
                Vector3 end = ends[i] + cameraPush;
                activeTraces.Add(new ActiveTrace(lr, cam, start, end, safeWidthPixels, safeDuration));
            }
        }

        void EnsureMaterial()
        {
            if (sharedMaterial != null)
                return;

            Shader shader = Shader.Find("Custom/AttackTraceOverlay");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            sharedMaterial = new Material(shader);
            sharedMaterial.color = Color.white;
            sharedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay;
        }
    }

    sealed class ActiveTrace
    {
        readonly LineRenderer lineRenderer;
        readonly Camera camera;
        readonly Vector3 start;
        readonly Vector3 end;
        readonly float widthPixels;
        readonly float startTime;
        readonly float duration;

        public ActiveTrace(LineRenderer lineRenderer, Camera camera, Vector3 start, Vector3 end, float widthPixels, float duration)
        {
            this.lineRenderer = lineRenderer;
            this.camera = camera;
            this.start = start;
            this.end = end;
            this.widthPixels = widthPixels;
            this.duration = duration;
            startTime = Time.time;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, start);
        }

        public bool Tick(float now)
        {
            if (lineRenderer == null)
                return false;

            UpdateWidth();
            float normalized = Mathf.Clamp01((now - startTime) / duration);
            if (normalized < 0.5f)
            {
                float t = normalized / 0.5f;
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, Vector3.Lerp(start, end, t));
                return true;
            }

            if (normalized < 1f)
            {
                float t = (normalized - 0.5f) / 0.5f;
                lineRenderer.SetPosition(0, Vector3.Lerp(start, end, t));
                lineRenderer.SetPosition(1, end);
                return true;
            }

            return false;
        }

        void UpdateWidth()
        {
            if (camera == null)
                return;

            Vector3 midpoint = (lineRenderer.GetPosition(0) + lineRenderer.GetPosition(1)) * 0.5f;
            float distance = Vector3.Dot(midpoint - camera.transform.position, camera.transform.forward);
            distance = Mathf.Max(camera.nearClipPlane + 0.05f, distance);
            float worldPerPixel = (2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad)) / Mathf.Max(1f, Screen.height);
            float widthWorld = Mathf.Max(0.001f, widthPixels * worldPerPixel);
            lineRenderer.startWidth = widthWorld;
            lineRenderer.endWidth = widthWorld;
        }

        public void Dispose()
        {
            if (lineRenderer != null)
                Object.Destroy(lineRenderer.gameObject);
        }
    }

    static AttackTraceHost host;

    public static void DrawChargeBurst(Vector3 worldCenter, Vector3 worldReference, float scale, float duration, float depth)
    {
        Camera cam = ResolveCamera();
        if (cam == null)
            return;

        if (!IsVisibleFromCamera(cam, worldCenter) && !IsVisibleFromCamera(cam, worldReference))
            return;

        BuildBillboardBasis(cam, worldCenter, worldReference - worldCenter, out Vector3 axis, out Vector3 ortho);

        var starts = new List<Vector3>(6);
        var ends = new List<Vector3>(6);
        float safeScale = ResolveWorldScale(cam, worldCenter, scale);
        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 dir = RotateOnBillboard(axis, ortho, angle);
            starts.Add(worldCenter);
            ends.Add(worldCenter + dir * safeScale);
        }

        EnsureHost().DrawSegments(cam, starts, ends, depth, duration);
    }

    public static void DrawBiteProjection(Vector3 worldCenter, float scale, float duration, float depth)
    {
        Camera cam = ResolveCamera();
        if (cam == null)
            return;

        if (!IsVisibleFromCamera(cam, worldCenter))
            return;

        float safeScale = ResolveWorldScale(cam, worldCenter, scale);
        Vector2[] fromOffsets =
        {
            new Vector2(-1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-1f, -1f),
            new Vector2(0f, -1f),
            new Vector2(1f, -1f)
        };
        Vector2[] toOffsets =
        {
            new Vector2(-1f, 0f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-1f, 0f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        };

        BuildBillboardBasis(cam, worldCenter, cam.transform.right, out Vector3 axisX, out Vector3 axisY);
        var starts = new List<Vector3>(fromOffsets.Length);
        var ends = new List<Vector3>(fromOffsets.Length);
        for (int i = 0; i < fromOffsets.Length; i++)
        {
            starts.Add(worldCenter + ((axisX * fromOffsets[i].x) + (axisY * fromOffsets[i].y)) * safeScale);
            ends.Add(worldCenter + ((axisX * toOffsets[i].x) + (axisY * toOffsets[i].y)) * safeScale);
        }

        EnsureHost().DrawSegments(cam, starts, ends, depth, duration);
    }

    public static void DrawMeleeArc(Vector3 worldCenter, Vector3 worldForward, float radius, float arcDegrees, float scale, float duration, float depth, int segments = 8)
    {
        Camera cam = ResolveCamera();
        if (cam == null)
            return;

        if (!IsVisibleFromCamera(cam, worldCenter))
            return;

        BuildBillboardBasis(cam, worldCenter, worldForward, out Vector3 axis, out Vector3 ortho);
        float safeScale = ResolveWorldScale(cam, worldCenter, scale);
        float halfArc = Mathf.Max(1f, arcDegrees) * 0.5f;
        int safeSegments = Mathf.Max(2, segments);
        Vector3 prev = worldCenter + RotateOnBillboard(axis, ortho, -halfArc * Mathf.Deg2Rad) * safeScale;

        var starts = new List<Vector3>(safeSegments);
        var ends = new List<Vector3>(safeSegments);
        for (int i = 1; i <= safeSegments; i++)
        {
            float t = i / (float)safeSegments;
            float angle = Mathf.Lerp(-halfArc, halfArc, t) * Mathf.Deg2Rad;
            Vector3 next = worldCenter + RotateOnBillboard(axis, ortho, angle) * safeScale;
            starts.Add(prev);
            ends.Add(next);
            prev = next;
        }

        EnsureHost().DrawSegments(cam, starts, ends, depth, duration);
    }

    static Vector3 RotateOnBillboard(Vector3 axis, Vector3 ortho, float radians)
    {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return axis * cos + ortho * sin;
    }

    static void BuildBillboardBasis(Camera cam, Vector3 center, Vector3 preferredAxis, out Vector3 axis, out Vector3 ortho)
    {
        Vector3 viewNormal = (center - cam.transform.position).normalized;
        if (viewNormal.sqrMagnitude <= 0.0001f)
            viewNormal = cam.transform.forward;

        axis = Vector3.ProjectOnPlane(preferredAxis, viewNormal);
        if (axis.sqrMagnitude <= 0.0001f)
            axis = Vector3.ProjectOnPlane(cam.transform.right, viewNormal);
        if (axis.sqrMagnitude <= 0.0001f)
            axis = Vector3.ProjectOnPlane(Vector3.right, viewNormal);
        axis.Normalize();

        ortho = Vector3.Cross(viewNormal, axis).normalized;
        if (ortho.sqrMagnitude <= 0.0001f)
            ortho = Vector3.up;
    }

    static bool IsVisibleFromCamera(Camera cam, Vector3 worldPosition)
    {
        Vector3 viewport = cam.WorldToViewportPoint(worldPosition);
        return viewport.z > 0f;
    }

    static AttackTraceHost EnsureHost()
    {
        if (host != null)
            return host;

        GameObject obj = new GameObject("AttackTraceLibraryHost");
        Object.DontDestroyOnLoad(obj);
        host = obj.AddComponent<AttackTraceHost>();
        return host;
    }

    static Camera ResolveCamera()
    {
        if (CommonAttackVisualUIManager.Instance != null && CommonAttackVisualUIManager.Instance.mainCamera != null)
            return CommonAttackVisualUIManager.Instance.mainCamera;

        Camera cam = Camera.main;
        if (cam != null)
            return cam;

        return Object.FindFirstObjectByType<Camera>();
    }

    static float ResolveWidthPixels()
    {
        if (CommonAttackVisualUIManager.Instance != null)
            return Mathf.Max(0.5f, CommonAttackVisualUIManager.Instance.attackTraceWidthPixels);

        return 3f;
    }

    static float ResolveWorldScale(Camera cam, Vector3 worldCenter, float scalePixels)
    {
        float safePixels = Mathf.Max(1f, scalePixels);
        float distance = Vector3.Dot(worldCenter - cam.transform.position, cam.transform.forward);
        distance = Mathf.Max(cam.nearClipPlane + 0.05f, distance);
        float worldPerPixel = (2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)) / Mathf.Max(1f, Screen.height);
        return Mathf.Max(0.01f, safePixels * worldPerPixel);
    }
}
