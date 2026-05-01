using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class IceShaderVisibilityController : MonoBehaviour
{
    public Camera targetCamera;
    public float maxRenderDistance = 80f;
    public float checkInterval = 0.2f;
    public bool disableOutsideFrustum = true;

    Renderer cachedRenderer;
    float nextCheckTime;
    readonly Plane[] frustumPlanes = new Plane[6];

    void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + Mathf.Max(0.02f, checkInterval);
        UpdateRendererState();
    }

    void UpdateRendererState()
    {
        if (cachedRenderer == null)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            cachedRenderer.enabled = true;
            return;
        }

        Bounds bounds = cachedRenderer.bounds;
        float sqrDistance = (bounds.center - targetCamera.transform.position).sqrMagnitude;
        float maxDistance = Mathf.Max(1f, maxRenderDistance);
        bool inDistance = sqrDistance <= maxDistance * maxDistance;
        bool inFrustum = true;

        if (disableOutsideFrustum)
        {
            GeometryUtility.CalculateFrustumPlanes(targetCamera, frustumPlanes);
            inFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }

        cachedRenderer.enabled = inDistance && inFrustum;
    }
}
