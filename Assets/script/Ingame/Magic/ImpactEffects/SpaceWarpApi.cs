using UnityEngine;

public readonly struct SpaceWarpPortalPair
{
    public readonly GameObject sourceWarp;
    public readonly GameObject destinationWarp;
    public readonly Camera sourceViewCamera;
    public readonly Camera destinationViewCamera;

    public SpaceWarpPortalPair(
        GameObject sourceWarp,
        GameObject destinationWarp,
        Camera sourceViewCamera,
        Camera destinationViewCamera)
    {
        this.sourceWarp = sourceWarp;
        this.destinationWarp = destinationWarp;
        this.sourceViewCamera = sourceViewCamera;
        this.destinationViewCamera = destinationViewCamera;
    }
}

public static class SpaceWarpApi
{
    public const string CustomShaderName = "MagicalEcosystem/Lab/SpaceWarpCustomMvp";
    const int PortalTextureSize = 512;
    const int PortalHoleCount = 8;

    public static SpaceWarpPortalPair CreateTeleportWarp(Vector3 sourcePoint, Vector3 destinationPoint)
    {
        return CreateTeleportWarp(sourcePoint, destinationPoint, 2.5f, 5f);
    }

    public static SpaceWarpPortalPair CreateTeleportWarp(
        Vector3 sourcePoint,
        Vector3 destinationPoint,
        float radius,
        float lifetime)
    {
        Vector3 travel = destinationPoint - sourcePoint;
        Vector3 sourceNormal = travel.sqrMagnitude > 0.001f ? -travel.normalized : Vector3.up;
        Vector3 destinationNormal = travel.sqrMagnitude > 0.001f ? travel.normalized : Vector3.up;

        GameObject sourceWarp = SpaceWarpImpactEffect.CreateWarp(
            sourcePoint,
            sourceNormal,
            radius,
            lifetime,
            CustomShaderName);
        sourceWarp.name = "Space Warp Source";

        GameObject destinationWarp = SpaceWarpImpactEffect.CreateWarp(
            destinationPoint,
            destinationNormal,
            radius,
            lifetime,
            CustomShaderName);
        destinationWarp.name = "Space Warp Destination";

        Camera mainCamera = Camera.main;
        Camera sourceViewCamera = CreatePortalCamera(
            "Space Warp Source View Camera",
            sourcePoint,
            destinationPoint,
            mainCamera);
        Camera destinationViewCamera = CreatePortalCamera(
            "Space Warp Destination View Camera",
            destinationPoint,
            sourcePoint,
            mainCamera);

        SpaceWarpPortalProjectionController projection = sourceWarp.AddComponent<SpaceWarpPortalProjectionController>();
        projection.Initialize(
            sourceWarp,
            destinationWarp,
            sourceViewCamera,
            destinationViewCamera,
            sourcePoint,
            destinationPoint,
            mainCamera);

        return new SpaceWarpPortalPair(sourceWarp, destinationWarp, sourceViewCamera, destinationViewCamera);
    }

    static Camera CreatePortalCamera(string name, Vector3 portalPoint, Vector3 oppositePoint, Camera mainCamera)
    {
        GameObject cameraObject = new GameObject(name);
        cameraObject.hideFlags = HideFlags.DontSave;
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.enabled = true;
        camera.fieldOfView = mainCamera != null ? mainCamera.fieldOfView : 60f;
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = mainCamera != null ? mainCamera.farClipPlane : 1000f;
        camera.clearFlags = mainCamera != null ? mainCamera.clearFlags : CameraClearFlags.Skybox;
        camera.backgroundColor = mainCamera != null ? mainCamera.backgroundColor : Color.black;
        camera.transform.position = oppositePoint;
        camera.transform.rotation = mainCamera != null
            ? mainCamera.transform.rotation
            : Quaternion.LookRotation(portalPoint - oppositePoint, Vector3.up);
        camera.targetTexture = CreatePortalTexture(name);
        return camera;
    }

    static RenderTexture CreatePortalTexture(string name)
    {
        RenderTexture texture = new RenderTexture(PortalTextureSize, PortalTextureSize, 16, RenderTextureFormat.ARGB32)
        {
            name = $"{name} Texture",
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false
        };
        texture.Create();
        return texture;
    }
}

sealed class SpaceWarpPortalProjectionController : MonoBehaviour
{
    GameObject sourceWarp;
    GameObject destinationWarp;
    Camera sourceViewCamera;
    Camera destinationViewCamera;
    Camera viewerCamera;
    Vector3 sourcePoint;
    Vector3 destinationPoint;
    Renderer sourceRenderer;
    Renderer destinationRenderer;
    PortalCameraRenderGuard sourceGuard;
    PortalCameraRenderGuard destinationGuard;
    Vector4[] sourceHoleCenters;
    Vector4[] destinationHoleCenters;
    float revealElapsed;
    const int PortalHoleCount = 8;
    const float RevealDelay = 0.5f;
    const float RevealDuration = 3.2f;

    public void Initialize(
        GameObject sourceWarpObject,
        GameObject destinationWarpObject,
        Camera sourceCamera,
        Camera destinationCamera,
        Vector3 source,
        Vector3 destination,
        Camera mainCamera)
    {
        sourceWarp = sourceWarpObject;
        destinationWarp = destinationWarpObject;
        sourceViewCamera = sourceCamera;
        destinationViewCamera = destinationCamera;
        sourcePoint = source;
        destinationPoint = destination;
        viewerCamera = mainCamera != null ? mainCamera : Camera.main;

        sourceRenderer = sourceWarp != null ? sourceWarp.GetComponentInChildren<Renderer>() : null;
        destinationRenderer = destinationWarp != null ? destinationWarp.GetComponentInChildren<Renderer>() : null;

        sourceGuard = AddRenderGuard(sourceViewCamera);
        destinationGuard = AddRenderGuard(destinationViewCamera);
        sourceHoleCenters = CreateHoleCenters(731);
        destinationHoleCenters = CreateHoleCenters(1543);
        ConfigurePortalMaterials();
        UpdatePortalCameras();
    }

    void LateUpdate()
    {
        if (viewerCamera == null)
            viewerCamera = Camera.main;

        revealElapsed += Time.deltaTime;
        UpdatePortalReveal();
        UpdatePortalCameras();
    }

    void ConfigurePortalMaterials()
    {
        if (sourceRenderer != null && sourceRenderer.material != null && destinationViewCamera != null)
        {
            ApplyPortalTexture(sourceRenderer.material, destinationViewCamera.targetTexture);
            ApplyPortalHoles(sourceRenderer.material, sourceHoleCenters);
        }

        if (destinationRenderer != null && destinationRenderer.material != null && sourceViewCamera != null)
        {
            ApplyPortalTexture(destinationRenderer.material, sourceViewCamera.targetTexture);
            ApplyPortalHoles(destinationRenderer.material, destinationHoleCenters);
        }

        Renderer[] hiddenRenderers = { sourceRenderer, destinationRenderer };
        if (sourceGuard != null)
            sourceGuard.hiddenRenderers = hiddenRenderers;
        if (destinationGuard != null)
            destinationGuard.hiddenRenderers = hiddenRenderers;
    }

    static void ApplyPortalTexture(Material material, Texture texture)
    {
        if (material.HasProperty("_PortalTex"))
            material.SetTexture("_PortalTex", texture);
        if (material.HasProperty("_PortalBlend"))
            material.SetFloat("_PortalBlend", 1f);
        if (material.HasProperty("_PortalTint"))
            material.SetColor("_PortalTint", Color.white);
        if (material.HasProperty("_WarpPower"))
            material.SetFloat("_WarpPower", 1f);
        if (material.HasProperty("_PortalHoleCount"))
            material.SetFloat("_PortalHoleCount", PortalHoleCount);
        if (material.HasProperty("_PortalHoleMaxRadius"))
            material.SetFloat("_PortalHoleMaxRadius", 0.72f);
        if (material.HasProperty("_PortalHoleSoftness"))
            material.SetFloat("_PortalHoleSoftness", 0.08f);
    }

    static void ApplyPortalHoles(Material material, Vector4[] centers)
    {
        if (material == null || centers == null)
            return;

        material.SetVectorArray("_PortalHoleCenters", centers);
        if (material.HasProperty("_PortalHoleProgress"))
            material.SetFloat("_PortalHoleProgress", 0f);
    }

    static Vector4[] CreateHoleCenters(int seed)
    {
        Random.State previousState = Random.state;
        Random.InitState(seed);
        Vector4[] centers = new Vector4[PortalHoleCount];
        for (int i = 0; i < centers.Length; i++)
        {
            Vector3 direction = Random.onUnitSphere;
            centers[i] = new Vector4(direction.x, direction.y, direction.z, 0f);
        }

        Random.state = previousState;
        return centers;
    }

    void UpdatePortalReveal()
    {
        float progress = Mathf.Clamp01((revealElapsed - RevealDelay) / RevealDuration);
        SetPortalRevealProgress(sourceRenderer, progress);
        SetPortalRevealProgress(destinationRenderer, progress);
    }

    static void SetPortalRevealProgress(Renderer renderer, float progress)
    {
        if (renderer == null || renderer.material == null)
            return;

        Material material = renderer.material;
        if (material.HasProperty("_PortalHoleProgress"))
            material.SetFloat("_PortalHoleProgress", progress);
    }

    void UpdatePortalCameras()
    {
        if (viewerCamera == null)
            return;

        UpdateCamera(sourceViewCamera, sourcePoint, destinationPoint);
        UpdateCamera(destinationViewCamera, destinationPoint, sourcePoint);
    }

    void UpdateCamera(Camera portalCamera, Vector3 fromPoint, Vector3 toPoint)
    {
        if (portalCamera == null)
            return;

        portalCamera.transform.position = viewerCamera.transform.position;
        portalCamera.fieldOfView = viewerCamera.fieldOfView;
        portalCamera.nearClipPlane = viewerCamera.nearClipPlane;
        portalCamera.farClipPlane = viewerCamera.farClipPlane;
    }

    static PortalCameraRenderGuard AddRenderGuard(Camera camera)
    {
        if (camera == null)
            return null;

        PortalCameraRenderGuard guard = camera.GetComponent<PortalCameraRenderGuard>();
        if (guard == null)
            guard = camera.gameObject.AddComponent<PortalCameraRenderGuard>();
        return guard;
    }

    void OnDestroy()
    {
        DestroyPortalCamera(sourceViewCamera);
        DestroyPortalCamera(destinationViewCamera);
    }

    static void DestroyPortalCamera(Camera camera)
    {
        if (camera == null)
            return;

        RenderTexture texture = camera.targetTexture;
        camera.targetTexture = null;
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }

        Destroy(camera.gameObject);
    }
}

sealed class PortalCameraRenderGuard : MonoBehaviour
{
    public Renderer[] hiddenRenderers;

    void OnPreCull()
    {
        SetRenderersEnabled(false);
    }

    void OnPostRender()
    {
        SetRenderersEnabled(true);
    }

    void OnDisable()
    {
        SetRenderersEnabled(true);
    }

    void SetRenderersEnabled(bool enabled)
    {
        if (hiddenRenderers == null)
            return;

        for (int i = 0; i < hiddenRenderers.Length; i++)
        {
            if (hiddenRenderers[i] != null)
                hiddenRenderers[i].enabled = enabled;
        }
    }
}
