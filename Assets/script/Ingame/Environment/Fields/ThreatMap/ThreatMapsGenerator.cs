using System.Collections.Generic;
using UnityEngine;

public sealed class ThreatMapsGenerator : threatmap_calc
{
    [Header("Debug")]
    public bool showThreatMap = false;
    public float threatColumnHeightScale = 0.8f;

    readonly List<GameObject> threatDebugColumns = new();

    void OnDestroy()
    {
        for (int i = 0; i < threatDebugColumns.Count; i++)
        {
            if (threatDebugColumns[i] != null)
                UnityEngine.Object.Destroy(threatDebugColumns[i]);
        }
        threatDebugColumns.Clear();
    }

    protected override void OnMapEvaluated(int count, in Settings settings, Transform debugOwner)
    {
        UpdateThreatDebugColumns(showThreatMap, count, settings, debugOwner);
    }

    void UpdateThreatDebugColumns(bool show, int count, in Settings settings, Transform debugOwner)
    {
        if (!show)
        {
            HideThreatDebugColumns();
            return;
        }

        EnsureThreatDebugColumns(count, debugOwner);
        float safeScale = Mathf.Max(0.05f, threatColumnHeightScale);
        float cellScale = Mathf.Max(0.2f, settings.cellSize * 0.85f);

        for (int i = 0; i < count; i++)
        {
            GameObject column = threatDebugColumns[i];
            if (column == null) continue;

            float h = Mathf.Max(0.05f, predatorFieldBuffer[i] * safeScale);
            Vector3 p = cellPositionBuffer[i];
            column.transform.position = new Vector3(p.x, p.y + h * 0.5f, p.z);
            column.transform.localScale = new Vector3(cellScale, h, cellScale);
            column.SetActive(true);

            var renderer = column.GetComponent<Renderer>();
            if (renderer != null)
            {
                float alpha = Mathf.Clamp01(predatorFieldBuffer[i] / 6f) * 0.5f + 0.1f;
                renderer.material.color = new Color(1f, 0.15f, 0.1f, alpha);
            }
        }

        for (int i = count; i < threatDebugColumns.Count; i++)
        {
            if (threatDebugColumns[i] != null)
                threatDebugColumns[i].SetActive(false);
        }
    }

    void EnsureThreatDebugColumns(int count, Transform debugOwner)
    {
        while (threatDebugColumns.Count < count)
        {
            GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cube);
            column.name = "ThreatMapColumn";
            if (debugOwner != null)
                column.transform.SetParent(debugOwner, true);
            column.layer = LayerMask.NameToLayer("Default");
            if (column.TryGetComponent<Collider>(out var c))
                UnityEngine.Object.Destroy(c);
            threatDebugColumns.Add(column);
        }
    }

    void HideThreatDebugColumns()
    {
        for (int i = 0; i < threatDebugColumns.Count; i++)
        {
            if (threatDebugColumns[i] != null)
                threatDebugColumns[i].SetActive(false);
        }
    }
}
