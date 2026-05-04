using UnityEngine;

public static class CustomMagicCircleShaderManager
{
    const string ShaderName = "MagicalEcosystem/Experiment/SpellCircleFractal";

    public static GameObject CreateLaunchCircle(
        MagicElement element,
        Vector3 launchPoint,
        Vector3 launchDirection,
        Color lineColor,
        MagicCircleLaunchSettings settings)
    {
        if (!settings.enabled)
            return null;

        Vector3 direction = launchDirection.sqrMagnitude > 0.001f ? launchDirection.normalized : Vector3.forward;
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Plane);
        circle.name = $"{element} Spell Circle";
        circle.transform.position = launchPoint + direction * settings.forwardOffset;
        circle.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        circle.transform.localScale = Vector3.one * Mathf.Max(0.01f, settings.radius * 0.2f);

        Collider collider = circle.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        Renderer renderer = circle.GetComponent<Renderer>();
        renderer.material = CreateMaterial(element, lineColor, settings);

        Object.Destroy(circle, Mathf.Max(0.05f, settings.visibleLifetime));
        return circle;
    }

    static Material CreateMaterial(MagicElement element, Color lineColor, MagicCircleLaunchSettings settings)
    {
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        Color color = lineColor;
        color.a = settings.lineColor.a > 0f ? settings.lineColor.a : 0.5f;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        SetFloat(material, "_Sides", GetPolygonSideCount(element));
        SetFloat(material, "_LineWidth", Mathf.Max(0.001f, settings.lineWidth));
        SetFloat(material, "_MinScale", Mathf.Clamp(settings.minFractalScale, 0.02f, 0.95f));
        SetFloat(material, "_MaxIterations", Mathf.Clamp(settings.maxFractalIterations, 1, 10));
        SetFloat(material, "_SpawnTime", Time.time);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    static void SetFloat(Material material, string name, float value)
    {
        if (material.HasProperty(name))
            material.SetFloat(name, value);
    }

    static int GetPolygonSideCount(MagicElement element)
    {
        switch (element)
        {
            case MagicElement.Fire:
                return 3;
            case MagicElement.Ice:
                return 6;
            case MagicElement.Lightning:
                return 4;
            case MagicElement.Wind:
                return 12;
            case MagicElement.Space:
                return 2;
            default:
                return 12;
        }
    }
}
