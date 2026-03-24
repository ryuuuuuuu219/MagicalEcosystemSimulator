using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class grassland : MonoBehaviour
{
    public GameObject plant;
    public TerrainData tdata;

    List<GameObject> plants = new List<GameObject>();

    public float waterHeight = 8f;
    public int spawnCount = 100;
    public float radius = 20f;
    public float density = 0.2f;
    public float maxSlope = 30f;

    public void setting(float waterHeight, int spawnCount, float radius, float density, float maxSlope)
    {
        this.waterHeight = waterHeight;
        this.spawnCount = spawnCount;
        this.radius = radius;
        this.density = density;
        this.maxSlope = maxSlope;
    }

    public void setseed(System.Random rng)
    {
        this.rng = rng;
    }
    System.Random rng;

    public void ready()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = radius;
        col.isTrigger = true; // トリガーに設定して、物理的な衝突を無効にする
        gameObject.layer = LayerMask.NameToLayer("Grassland");

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // 物理演算の影響を受けないようにする


        Vector3 center = transform.position;
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject obj = SetPlantObj();
            plants.Add(obj);
            obj.transform.parent = transform;

            Vector2 offset = RandomPointInCircle(radius);

            float worldX = center.x + offset.x;
            float worldZ = center.z + offset.y;

            float normX = worldX / tdata.size.x;
            float normZ = worldZ / tdata.size.z;

            float height = tdata.GetInterpolatedHeight(normX, normZ);
            float slope = tdata.GetSteepness(normX, normZ);

            if (height <= waterHeight) continue;
            if (slope > maxSlope) continue;

            Vector3 spawnPos = new Vector3(worldX, height, worldZ);

            plants[i].transform.position = spawnPos;
            plants[i].transform.rotation = Quaternion.Euler(0, rng.Next(0, 360), 0);
        }
    }

    private void Update()
    {

        Debug.DrawLine(transform.position, transform.position + Vector3.up * 5f, Color.green);

        int currentPlantCount = spawnCount;
        for (int i = 0; i < spawnCount; i++)
        {
            plants[i].SetActive(true);
        }
        for (int i = spawnCount; i < plants.Count; i++)
        {
            plants[i].SetActive(false);
        }
    }


    GameObject SetPlantObj()//緑のペラペラなオブジェクトを返すだけの関数。
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.name = "glassland";

        obj.transform.localScale = Vector3.one * 0.5f;

        var renderer = obj.GetComponent<MeshRenderer>();
        renderer.material = CreatePlantMaterial();

        Destroy(obj.GetComponent<Collider>());

        obj.SetActive(false); // ← 重要
        return obj;
    }
    Vector2 RandomPointInCircle(float r)
    {
        float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
        float dist = Mathf.Sqrt((float)rng.NextDouble()) * r;

        return new Vector2(
            Mathf.Cos(angle) * dist,
            Mathf.Sin(angle) * dist
        );
    }
    Material CreatePlantMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);

        Color plantColor = new Color(0.5f, 1f, 0.5f, 0.7f); // 半透明の緑色

        mat.SetColor("_BaseColor", plantColor);
        mat.SetFloat("_Smoothness", 0.2f);
        mat.SetFloat("_Metallic", 0f);

        return mat;
    }
}
