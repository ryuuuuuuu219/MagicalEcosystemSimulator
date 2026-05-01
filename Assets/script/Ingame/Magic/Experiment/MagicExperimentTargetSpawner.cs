using UnityEngine;

public class MagicExperimentTargetSpawner : MonoBehaviour
{
    public Camera sourceCamera;
    public int targetCount = 6;
    public int randomSeed = 1234;
    public float minDistance = 14f;
    public float maxDistance = 28f;
    public Vector2 horizontalRange = new Vector2(-8f, 8f);
    public Vector2 verticalRange = new Vector2(-3f, 4f);
    public Vector3 minTargetSize = new Vector3(2f, 2f, 0.8f);
    public Vector3 maxTargetSize = new Vector3(5f, 5f, 1.4f);

    void Start()
    {
        if (sourceCamera == null)
            sourceCamera = Camera.main;

        SpawnTargets();
    }

    public void SpawnTargets()
    {
        if (sourceCamera == null)
            return;

        if (GameObject.Find("Magic Experiment Target 00") != null)
            return;

        Random.InitState(randomSeed);

        for (int i = 0; i < Mathf.Max(0, targetCount); i++)
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = $"Magic Experiment Target {i:00}";
            target.transform.position = GetRandomSpawnPosition();
            target.transform.localScale = GetRandomScale();
            target.transform.rotation = Quaternion.LookRotation(sourceCamera.transform.forward, Vector3.up);

            var renderer = target.GetComponent<Renderer>();
            renderer.material = CreateTargetMaterial(i);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Transform cam = sourceCamera.transform;
        float distance = Random.Range(minDistance, maxDistance);
        float horizontal = Random.Range(horizontalRange.x, horizontalRange.y);
        float vertical = Random.Range(verticalRange.x, verticalRange.y);

        return cam.position
            + cam.forward * distance
            + cam.right * horizontal
            + cam.up * vertical;
    }

    Vector3 GetRandomScale()
    {
        return new Vector3(
            Random.Range(minTargetSize.x, maxTargetSize.x),
            Random.Range(minTargetSize.y, maxTargetSize.y),
            Random.Range(minTargetSize.z, maxTargetSize.z));
    }

    static Material CreateTargetMaterial(int index)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color color = Color.HSVToRGB(Mathf.Repeat(0.53f + index * 0.06f, 1f), 0.55f, 0.95f);
        color.a = 0.35f;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.65f);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
