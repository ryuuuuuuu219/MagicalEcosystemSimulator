using UnityEngine;

public static class MagicImpactEnvelopeEffect
{
    public static GameObject Create(Collider target, Color color, float lifetime, float padding)
    {
        Bounds bounds = target.bounds;
        GameObject envelope = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        envelope.name = $"{target.name} Magic Material Envelope";
        envelope.transform.position = bounds.center;

        float diameter = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) + padding * 2f;
        envelope.transform.localScale = Vector3.one * Mathf.Max(0.1f, diameter);

        Collider envelopeCollider = envelope.GetComponent<Collider>();
        if (envelopeCollider != null)
            Object.Destroy(envelopeCollider);

        Renderer renderer = envelope.GetComponent<Renderer>();
        renderer.material = CreateEnvelopeMaterial(color);

        Object.Destroy(envelope, Mathf.Max(0.1f, lifetime));
        return envelope;
    }

    static Material CreateEnvelopeMaterial(Color source)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color color = new Color(source.r, source.g, source.b, Mathf.Clamp(source.a * 0.45f, 0.18f, 0.5f));

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.8f);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
