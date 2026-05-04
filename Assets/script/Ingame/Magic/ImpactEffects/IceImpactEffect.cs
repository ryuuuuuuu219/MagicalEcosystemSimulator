using UnityEngine;

public static class IceImpactEffect
{
    static Texture2D iceNoiseTexture;

    public static GameObject CreateSpike(Vector3 point, Vector3 normal, float height, float radius, float lifetime)
    {
        GameObject spike = new GameObject("Ice Spike Impact");
        var meshFilter = spike.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = CreateHexSpikeMesh(height, radius);

        var renderer = spike.AddComponent<MeshRenderer>();
        renderer.material = CreateIceMaterial();

        var visibility = spike.AddComponent<IceShaderVisibilityController>();
        visibility.maxRenderDistance = 80f;
        visibility.checkInterval = 0.2f;
        visibility.disableOutsideFrustum = true;

        var collider = spike.AddComponent<MeshCollider>();
        collider.sharedMesh = meshFilter.sharedMesh;
        collider.convex = true;
        collider.isTrigger = true;

        spike.name = "Ice Spike Impact";
        spike.transform.position = point + normal.normalized * (height * 0.5f);
        spike.transform.up = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;

        Object.Destroy(spike, Mathf.Max(0.1f, lifetime));
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
            triangles[t++] = topNext;
            triangles[t++] = top;

            triangles[t++] = 0;
            triangles[t++] = bottom;
            triangles[t++] = bottomNext;
        }

        Mesh mesh = new Mesh();
        mesh.name = "IceSpike_HexPrismDoubleCone";
        ApplyFlatTriangles(mesh, vertices, triangles);
        mesh.RecalculateBounds();
        return mesh;
    }

    static void ApplyFlatTriangles(Mesh mesh, Vector3[] sourceVertices, int[] sourceTriangles)
    {
        Vector3[] flatVertices = new Vector3[sourceTriangles.Length];
        int[] flatTriangles = new int[sourceTriangles.Length];

        for (int i = 0; i < sourceTriangles.Length; i++)
        {
            flatVertices[i] = sourceVertices[sourceTriangles[i]];
            flatTriangles[i] = i;
        }

        mesh.vertices = flatVertices;
        mesh.triangles = flatTriangles;
        mesh.RecalculateNormals();
    }

    static Material CreateIceMaterial()
    {
        Shader shader = Shader.Find("MagicalEcosystem/Experiment/IceRefraction");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        if (material.HasProperty("_LightBlue"))
            material.SetColor("_LightBlue", new Color(0.55f, 0.9f, 1f, 1f));
        if (material.HasProperty("_DeepBlue"))
            material.SetColor("_DeepBlue", new Color(0.02f, 0.22f, 0.55f, 1f));
        if (material.HasProperty("_IceWhite"))
            material.SetColor("_IceWhite", new Color(0.92f, 0.98f, 1f, 1f));
        if (material.HasProperty("_BlueAmount"))
            material.SetFloat("_BlueAmount", 0.42f);
        if (material.HasProperty("_FresnelPower"))
            material.SetFloat("_FresnelPower", 4f);
        if (material.HasProperty("_ThicknessScale"))
            material.SetFloat("_ThicknessScale", 0.75f);
        if (material.HasProperty("_DistortionStrength"))
            material.SetFloat("_DistortionStrength", 0.015f);
        if (material.HasProperty("_NoiseScale"))
            material.SetFloat("_NoiseScale", 4f);
        if (material.HasProperty("_NoiseTex"))
            material.SetTexture("_NoiseTex", GetOrCreateIceNoiseTexture());
        if (material.HasProperty("_NoiseDistortionStrength"))
            material.SetFloat("_NoiseDistortionStrength", 0.012f);
        if (material.HasProperty("_CrackStrength"))
            material.SetFloat("_CrackStrength", 0.22f);
        if (material.HasProperty("_Alpha"))
            material.SetFloat("_Alpha", 0.42f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    static Texture2D GetOrCreateIceNoiseTexture()
    {
        if (iceNoiseTexture != null)
            return iceNoiseTexture;

        const int size = 64;
        iceNoiseTexture = new Texture2D(size, size, TextureFormat.RGBA32, true);
        iceNoiseTexture.name = "Runtime_IceNoise";
        iceNoiseTexture.wrapMode = TextureWrapMode.Repeat;
        iceNoiseTexture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = x / (float)size;
                float ny = y / (float)size;
                float coarse = Mathf.PerlinNoise(nx * 4.7f + 13.2f, ny * 4.7f + 41.7f);
                float fine = Mathf.PerlinNoise(nx * 18.3f + 2.1f, ny * 18.3f + 9.6f);
                float crack = Mathf.PerlinNoise((nx + ny) * 22.0f, (ny - nx) * 22.0f + 5.5f);
                float fog = Mathf.Clamp01(coarse * 0.75f + fine * 0.25f);
                iceNoiseTexture.SetPixel(x, y, new Color(fog, crack, fine, 1f));
            }
        }

        iceNoiseTexture.Apply();
        return iceNoiseTexture;
    }
}
