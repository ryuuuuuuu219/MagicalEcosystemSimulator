using UnityEngine;

public static class IceImpactEffect
{
    public static GameObject CreateSpike(Vector3 point, Vector3 normal, float height, float radius)
    {
        GameObject spike = new GameObject("Ice Spike Impact");
        var meshFilter = spike.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateHexSpikeMesh(height, radius);

        var renderer = spike.AddComponent<MeshRenderer>();
        renderer.material = CreateIceMaterial();

        var collider = spike.AddComponent<MeshCollider>();
        collider.sharedMesh = meshFilter.sharedMesh;
        collider.convex = true;
        collider.isTrigger = true;

        spike.name = "Ice Spike Impact";
        spike.transform.position = point + normal.normalized * (height * 0.5f);
        spike.transform.up = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;

        return spike;
    }

    static Mesh CreateHexSpikeMesh(float height, float radius)
    {
        const int sides = 6;
        float prismHalfHeight = height * 0.25f;
        float tipHeight = height * 0.5f;

        Vector3[] vertices = new Vector3[2 + sides * 2];
        vertices[0] = Vector3.down * tipHeight;
        vertices[1] = Vector3.up * tipHeight;

        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.PI * 2f * i / sides;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices[2 + i] = new Vector3(x, -prismHalfHeight, z);
            vertices[2 + sides + i] = new Vector3(x, prismHalfHeight, z);
        }

        int[] triangles = new int[sides * 12];
        int t = 0;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            int bottom = 2 + i;
            int bottomNext = 2 + next;
            int top = 2 + sides + i;
            int topNext = 2 + sides + next;

            triangles[t++] = bottom;
            triangles[t++] = top;
            triangles[t++] = topNext;
            triangles[t++] = bottom;
            triangles[t++] = topNext;
            triangles[t++] = bottomNext;

            triangles[t++] = 1;
            triangles[t++] = top;
            triangles[t++] = topNext;

            triangles[t++] = 0;
            triangles[t++] = bottomNext;
            triangles[t++] = bottom;
        }

        Mesh mesh = new Mesh();
        mesh.name = "IceSpike_HexPrismDoubleCone";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    static Material CreateIceMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        Color color = new Color(0.55f, 0.9f, 1f, 0.45f);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 0.75f);
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
