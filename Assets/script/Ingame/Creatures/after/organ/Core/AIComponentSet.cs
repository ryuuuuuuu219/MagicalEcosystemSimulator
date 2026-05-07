using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIComponentSet
{
    public List<AIComponentGene> genes = new();

    public bool TryGetGene(string componentId, out AIComponentGene gene)
    {
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].componentId == componentId)
            {
                gene = genes[i];
                return true;
            }
        }

        gene = default;
        return false;
    }

    public float GetLevel(string componentId, float fallback = 1f)
    {
        return TryGetGene(componentId, out AIComponentGene gene) && gene.enabled
            ? Mathf.Max(0f, gene.level)
            : fallback;
    }
}
