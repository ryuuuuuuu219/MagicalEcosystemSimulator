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
        return TryGetGene(componentId, out AIComponentGene gene) && gene.IsActive
            ? Mathf.Max(0f, gene.level)
            : fallback;
    }

    public float GetWeight(string componentId, float fallback = 1f)
    {
        return TryGetGene(componentId, out AIComponentGene gene) && gene.IsActive
            ? Mathf.Max(0f, gene.weight)
            : fallback;
    }

    public bool IsActive(string componentId, bool fallback = true)
    {
        return TryGetGene(componentId, out AIComponentGene gene)
            ? gene.IsActive
            : fallback;
    }

    public AIComponentGene EnsureGene(string componentId, bool enabled = true, bool isVitalOrgan = false)
    {
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].componentId != componentId)
                continue;

            AIComponentGene existing = ClampGene(genes[i]);
            if (isVitalOrgan && !existing.isVitalOrgan)
            {
                existing.isVitalOrgan = true;
            }

            if (enabled || existing.isVitalOrgan)
            {
                existing.enabled = true;
                existing.isVestigialOrgan = false;
                existing.weight = Mathf.Max(1f, existing.weight);
                existing.level = Mathf.Max(1f, existing.level);
            }

            genes[i] = existing;
            return existing;
        }

        AIComponentGene gene = AIComponentGene.CreateDefault(componentId, enabled, isVitalOrgan);
        genes.Add(gene);
        return gene;
    }

    public void SetGene(AIComponentGene gene)
    {
        gene = ClampGene(gene);
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].componentId == gene.componentId)
            {
                genes[i] = gene;
                return;
            }
        }

        genes.Add(gene);
    }

    public void ApplyPresetGene(AIComponentGene presetGene)
    {
        presetGene = ClampGene(presetGene);
        for (int i = 0; i < genes.Count; i++)
        {
            if (genes[i].componentId != presetGene.componentId)
                continue;

            AIComponentGene existing = ClampGene(genes[i]);
            if (presetGene.isVitalOrgan)
            {
                existing.isVitalOrgan = true;
                existing.enabled = true;
                existing.isVestigialOrgan = false;
                existing.level = Mathf.Max(1f, existing.level, presetGene.level);
                existing.weight = Mathf.Max(1f, existing.weight, presetGene.weight);
            }
            else if (presetGene.enabled && !existing.isVestigialOrgan)
            {
                existing.enabled = true;
                existing.level = Mathf.Max(existing.level, presetGene.level);
                existing.weight = Mathf.Max(existing.weight, presetGene.weight);
            }

            if (existing.mutationChanceT <= 0f)
                existing.mutationChanceT = presetGene.mutationChanceT;
            if (existing.mutationChanceG <= 0f)
                existing.mutationChanceG = presetGene.mutationChanceG;

            genes[i] = ClampGene(existing);
            return;
        }

        genes.Add(presetGene);
    }

    public bool TryMutateRuntime(string componentId, out AIComponentGene mutatedGene)
    {
        return TryMutateRuntime(componentId, 1f, out mutatedGene);
    }

    public bool TryMutateRuntime(string componentId, float chanceScale, out AIComponentGene mutatedGene)
    {
        if (!TryGetGene(componentId, out AIComponentGene gene) || gene.isVitalOrgan)
        {
            mutatedGene = default;
            return false;
        }

        float chance = Mathf.Clamp01(gene.mutationChanceT * Mathf.Max(0f, chanceScale));
        if (chance <= 0f || Random.value > chance)
        {
            mutatedGene = gene;
            return false;
        }

        gene = MutateGene(gene);
        SetGene(gene);
        mutatedGene = gene;
        return true;
    }

    public AIComponentSet CreateGenerationMutatedCopy(out bool changed)
    {
        return CreateGenerationMutatedCopy(out changed, out _);
    }

    public AIComponentSet CreateGenerationMutatedCopy(out bool changed, out List<AIComponentGene> mutatedGenes)
    {
        AIComponentSet copy = new AIComponentSet();
        mutatedGenes = new List<AIComponentGene>();
        changed = false;

        for (int i = 0; i < genes.Count; i++)
        {
            AIComponentGene gene = genes[i];
            if (!gene.isVitalOrgan && Mathf.Clamp01(gene.mutationChanceG) > 0f && Random.value <= Mathf.Clamp01(gene.mutationChanceG))
            {
                gene = MutateGene(gene);
                mutatedGenes.Add(gene);
                changed = true;
            }

            copy.SetGene(gene);
        }

        return copy;
    }

    public bool TryMutateGeneration(string componentId, out AIComponentGene mutatedGene)
    {
        if (!TryGetGene(componentId, out AIComponentGene gene) || gene.isVitalOrgan)
        {
            mutatedGene = default;
            return false;
        }

        float chance = Mathf.Clamp01(gene.mutationChanceG);
        if (chance <= 0f || Random.value > chance)
        {
            mutatedGene = gene;
            return false;
        }

        gene = MutateGene(gene);
        SetGene(gene);
        mutatedGene = gene;
        return true;
    }

    public void MarkVestigial(string componentId)
    {
        if (!TryGetGene(componentId, out AIComponentGene gene) || gene.isVitalOrgan)
            return;

        gene.enabled = false;
        gene.isVestigialOrgan = true;
        gene.weight = 0f;
        SetGene(gene);
    }

    public List<AIComponentGene> CloneGenes()
    {
        return new List<AIComponentGene>(genes);
    }

    static AIComponentGene MutateGene(AIComponentGene gene)
    {
        float min = Mathf.Min(gene.minLevel, gene.maxLevel);
        float max = Mathf.Max(gene.minLevel, gene.maxLevel);
        if (max <= 0f)
            max = 4f;

        float delta = Random.value < 0.5f ? -0.25f : 0.25f;
        gene.level = Mathf.Clamp(gene.level + delta, min, max);
        gene.weight = Mathf.Clamp(gene.weight + delta, 0f, max);

        if (gene.level <= 0f || gene.weight <= 0f)
        {
            gene.enabled = false;
            gene.isVestigialOrgan = true;
        }
        else if (Random.value < 0.1f)
        {
            gene.enabled = !gene.enabled;
            gene.isVestigialOrgan = !gene.enabled;
            if (!gene.enabled)
                gene.weight = 0f;
            else
                gene.weight = Mathf.Max(0.25f, gene.weight);
        }

        return ClampGene(gene);
    }

    static AIComponentGene ClampGene(AIComponentGene gene)
    {
        if (string.IsNullOrEmpty(gene.componentId))
            return gene;

        if (gene.maxLevel <= 0f)
            gene.maxLevel = 4f;
        gene.level = Mathf.Clamp(gene.level, Mathf.Min(gene.minLevel, gene.maxLevel), Mathf.Max(gene.minLevel, gene.maxLevel));
        gene.weight = Mathf.Max(0f, gene.weight);
        gene.mutationChanceT = Mathf.Clamp01(gene.mutationChanceT);
        gene.mutationChanceG = Mathf.Clamp01(gene.mutationChanceG);
        gene.installChance = Mathf.Clamp01(gene.installChance);

        if (gene.isVitalOrgan)
        {
            gene.enabled = true;
            gene.isVestigialOrgan = false;
            gene.level = Mathf.Max(1f, gene.level);
            gene.weight = Mathf.Max(1f, gene.weight);
        }

        return gene;
    }
}
