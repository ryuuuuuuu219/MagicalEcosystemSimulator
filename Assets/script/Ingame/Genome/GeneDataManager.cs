using System.Collections.Generic;
using UnityEngine;

public static class GeneDataManager
{
    public static List<ValueGene> genes_v = new();
    public static List<AIComponentGene> genes_s = new();
    public static List<GeneDataRecord> records = new();
    public static List<GeneDataRecord> checkpoints = new();

    public static void SetValueGene(ValueGene gene)
    {
        if (gene == null)
            return;

        for (int i = 0; i < genes_v.Count; i++)
        {
            if (genes_v[i] != null && genes_v[i].species == gene.species && genes_v[i].phase == gene.phase)
            {
                genes_v[i] = gene.Clone();
                return;
            }
        }

        genes_v.Add(gene.Clone());
    }

    public static void SetStructureGenes(List<AIComponentGene> genes)
    {
        genes_s = genes != null ? new List<AIComponentGene>(genes) : new List<AIComponentGene>();
    }

    public static GeneDataRecord SetCreatureValueGene(GameObject target, ValueGene gene, List<AIComponentGene> structureGenes = null)
    {
        if (target == null || gene == null)
            return null;

        category phase = gene.phase;
        int speciesID = gene.speciesID;
        if (target.TryGetComponent<Resource>(out var resource))
        {
            phase = resource.resourceCategory;
            speciesID = resource.speciesID;
        }

        gene.phase = phase;
        gene.speciesID = speciesID;

        GeneDataRecord record = GetOrCreateRecord(target);
        record.objectName = target.name;
        record.species = gene.species;
        record.phase = phase;
        record.speciesID = speciesID;
        record.valueGene = gene.Clone();
        record.fromCheckpoint = false;
        record.checkpointReason = string.Empty;
        record.checkpointTime = 0f;
        if (structureGenes != null)
            record.genes_s = new List<AIComponentGene>(structureGenes);
        else if (record.genes_s == null)
            record.genes_s = new List<AIComponentGene>();

        SetValueGene(gene);
        return record;
    }

    public static ValueGene GetValueGene(SpeciesType species, category phase)
    {
        for (int i = 0; i < genes_v.Count; i++)
        {
            ValueGene gene = genes_v[i];
            if (gene != null && gene.species == species && gene.phase == phase)
                return gene;
        }

        for (int i = 0; i < genes_v.Count; i++)
        {
            ValueGene gene = genes_v[i];
            if (gene != null && gene.species == species)
                return gene;
        }

        for (int i = 0; i < records.Count; i++)
        {
            GeneDataRecord record = records[i];
            if (record != null && record.valueGene != null && record.species == species && record.phase == phase)
                return record.valueGene;
        }

        for (int i = 0; i < records.Count; i++)
        {
            GeneDataRecord record = records[i];
            if (record != null && record.valueGene != null && record.species == species)
                return record.valueGene;
        }

        ValueGene fallback = CreateDefault(species, phase);
        SetValueGene(fallback);
        return fallback;
    }

    public static ValueGene SetFromHerbivoreGenome(HerbivoreGenome genome, category phase = category.herbivore, int speciesID = 0)
    {
        ValueGene gene = ValueGene.FromHerbivoreGenome(genome, phase, speciesID);
        SetValueGene(gene);
        return gene;
    }

    public static ValueGene SetFromPredatorGenome(PredatorGenome genome, category phase = category.predator, int speciesID = 0)
    {
        ValueGene gene = ValueGene.FromPredatorGenome(genome, phase, speciesID);
        SetValueGene(gene);
        return gene;
    }

    public static ValueGene SetCreatureFromHerbivoreGenome(GameObject target, HerbivoreGenome genome, category phase = category.herbivore, int speciesID = 0)
    {
        ValueGene gene = ValueGene.FromHerbivoreGenome(genome, phase, speciesID);
        SetCreatureValueGene(target, gene);
        return gene;
    }

    public static ValueGene SetCreatureFromPredatorGenome(GameObject target, PredatorGenome genome, category phase = category.predator, int speciesID = 0)
    {
        ValueGene gene = ValueGene.FromPredatorGenome(genome, phase, speciesID);
        SetCreatureValueGene(target, gene);
        return gene;
    }

    public static void ApplyToCreature(GameObject target)
    {
        if (target == null)
            return;

        category phase = category.herbivore;
        int speciesID = 0;
        if (target.TryGetComponent<Resource>(out var resource))
        {
            phase = resource.resourceCategory;
            speciesID = resource.speciesID;
        }

        SpeciesType species = GetPhaseRank(phase) >= GetPhaseRank(category.predator)
            ? SpeciesType.Predator
            : SpeciesType.Herbivore;

        GeneDataRecord record = GetRecord(target);
        ValueGene gene = record != null && record.valueGene != null
            ? record.valueGene
            : GetValueGene(species, phase);
        if (gene == null)
            return;

        gene.phase = phase;
        gene.speciesID = speciesID;
        SetCreatureValueGene(target, gene, record != null ? record.genes_s : null);
        GeneDataApplier.Apply(target, gene);
    }

    public static string ToJson()
    {
        GeneDataSnapshot snapshot = CreateSnapshot();
        return JsonUtility.ToJson(snapshot);
    }

    public static bool TryLoadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        string raw = json.Trim();
        if (raw.StartsWith(GenomeSerializer.HerbivorePrefix, System.StringComparison.Ordinal) ||
            raw.StartsWith("PGJ:", System.StringComparison.Ordinal) ||
            !raw.Contains("genes_v"))
        {
            return false;
        }

        try
        {
            GeneDataSnapshot snapshot = JsonUtility.FromJson<GeneDataSnapshot>(raw);
            if (snapshot == null)
                return false;
            if (snapshot.schema != "GeneDataManager" &&
                (snapshot.genes_v == null || snapshot.genes_v.Count == 0) &&
                (snapshot.records == null || snapshot.records.Count == 0))
                return false;

            genes_v = snapshot.genes_v != null ? snapshot.genes_v : new List<ValueGene>();
            genes_s = snapshot.genes_s != null ? snapshot.genes_s : new List<AIComponentGene>();
            records = snapshot.records != null ? CloneRecords(snapshot.records) : new List<GeneDataRecord>();
            checkpoints = snapshot.checkpoints != null ? CloneRecords(snapshot.checkpoints) : new List<GeneDataRecord>();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static GeneDataSnapshot CreateSnapshot()
    {
        return new GeneDataSnapshot
        {
            version = 1,
            schema = "GeneDataManager",
            genes_v = CloneValueGenes(genes_v),
            genes_s = new List<AIComponentGene>(genes_s),
            records = CloneRecords(records),
            checkpoints = CloneRecords(checkpoints)
        };
    }

    public static void MutateRuntimeValues(float chanceScale)
    {
        MutateValues(0.001f * Mathf.Max(0f, chanceScale));
    }

    public static void MutateRuntimeValues(GameObject target, float chanceScale)
    {
        if (target == null)
            return;

        GeneDataRecord record = GetRecord(target);
        if (record == null || record.valueGene == null)
            return;

        float chance = 0.001f * Mathf.Max(0f, chanceScale);
        if (chance <= 0f || Random.value > chance)
            return;

        MutateValue(record.valueGene, chance);
        GeneDataApplier.Apply(target, record.valueGene);
    }

    public static void MutateGenerationValues()
    {
        MutateValues(0.02f);
    }

    public static GeneDataRecord RecordCheckpoint(GameObject target, string reason, float time, List<AIComponentGene> structureGenes)
    {
        if (target == null)
            return null;

        GeneDataRecord source = GetRecord(target);
        if (source == null)
        {
            ApplyToCreature(target);
            source = GetRecord(target);
        }
        if (source == null)
            return null;

        GeneDataRecord checkpoint = source.Clone();
        checkpoint.instanceId = target.GetInstanceID();
        checkpoint.objectName = target.name;
        checkpoint.fromCheckpoint = true;
        checkpoint.checkpointReason = reason;
        checkpoint.checkpointTime = time;
        if (structureGenes != null)
            checkpoint.genes_s = new List<AIComponentGene>(structureGenes);

        checkpoints.Add(checkpoint);
        const int maxCheckpointRecords = 256;
        while (checkpoints.Count > maxCheckpointRecords)
            checkpoints.RemoveAt(0);
        return checkpoint;
    }

    public static GeneDataRecord GetRecord(GameObject target)
    {
        if (target == null)
            return null;

        int id = target.GetInstanceID();
        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] != null && records[i].instanceId == id)
                return records[i];
        }

        return null;
    }

    static void MutateValues(float chance)
    {
        if (chance <= 0f)
            return;

        for (int i = 0; i < genes_v.Count; i++)
        {
            ValueGene gene = genes_v[i];
            if (gene == null || Random.value > chance)
                continue;

            MutateValue(gene, chance);
            genes_v[i] = gene;
        }

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] == null || records[i].valueGene == null || Random.value > chance)
                continue;

            MutateValue(records[i].valueGene, chance);
        }
    }

    static void MutateValue(ValueGene gene, float chance)
    {
        if (gene == null || chance <= 0f)
            return;

        float scale = Random.value < 0.5f ? 0.95f : 1.05f;
        gene.Legacy.metabolismRate = MutateNonNegative(gene.Legacy.metabolismRate, scale);
        gene.Legacy.corpseWeight = MutatePositive(gene.Legacy.corpseWeight, scale);
        gene.Legacy.attackDistance = MutatePositive(gene.Legacy.attackDistance, scale);
        gene.Legacy.attackDamage = MutatePositive(gene.Legacy.attackDamage, scale);
        gene.Legacy.attackCooldown = MutatePositive(gene.Legacy.attackCooldown, scale);
        gene.GroundMotor.forwardForce = MutatePositive(gene.GroundMotor.forwardForce, scale);
        gene.GroundMotor.turnForce = MutatePositive(gene.GroundMotor.turnForce, scale);
        gene.FoodVisionSense.visionDistance = MutatePositive(gene.FoodVisionSense.visionDistance, scale);
        gene.PreyVisionSense.preyDetectDistance = MutatePositive(gene.PreyVisionSense.preyDetectDistance, scale);
        gene.PredatorVisionSense.threatDetectDistance = MutatePositive(gene.PredatorVisionSense.threatDetectDistance, scale);
        gene.ThreatVisionSense.threatDetectDistance = MutatePositive(gene.ThreatVisionSense.threatDetectDistance, scale);
        gene.BiteAttackAction.damage = MutatePositive(gene.BiteAttackAction.damage, scale);
        gene.MeleeAttackAction.damage = MutatePositive(gene.MeleeAttackAction.damage, scale);
        gene.ChargeAttackAction.damageScale = MutatePositive(gene.ChargeAttackAction.damageScale, scale);
    }

    static float MutatePositive(float value, float scale)
    {
        return value > 0f ? Mathf.Max(0.001f, value * scale) : value;
    }

    static float MutateNonNegative(float value, float scale)
    {
        return value >= 0f ? Mathf.Max(0f, value * scale) : value;
    }

    static void ApplyStructureGenes(GameObject target)
    {
        if (target == null || genes_s == null || genes_s.Count == 0)
            return;

        AIComponentSet set = new AIComponentSet();
        for (int i = 0; i < genes_s.Count; i++)
            set.SetGene(genes_s[i]);

        if (!target.TryGetComponent<AnimalAIInstaller>(out _))
            target.AddComponent<AnimalAIInstaller>();
        if (!target.TryGetComponent<OrganFoundation>(out var foundation))
            foundation = target.AddComponent<OrganFoundation>();

        foundation.InstallComponentSet(set, "gene data manager structure genes", true);
    }

    static ValueGene CreateDefault(SpeciesType species, category phase)
    {
        if (species == SpeciesType.Predator)
            return ValueGene.FromPredatorGenome(default, phase);
        return ValueGene.FromHerbivoreGenome(default, phase);
    }

    static GeneDataRecord GetOrCreateRecord(GameObject target)
    {
        GeneDataRecord record = GetRecord(target);
        if (record != null)
            return record;

        record = new GeneDataRecord
        {
            instanceId = target.GetInstanceID(),
            objectName = target.name,
            genes_s = new List<AIComponentGene>()
        };
        records.Add(record);
        return record;
    }

    static void ReplaceRecord(GeneDataRecord record)
    {
        if (record == null)
            return;

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] != null && records[i].instanceId == record.instanceId)
            {
                records[i] = record;
                return;
            }
        }

        records.Add(record);
    }

    static List<ValueGene> CloneValueGenes(List<ValueGene> source)
    {
        List<ValueGene> clone = new List<ValueGene>();
        if (source == null)
            return clone;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
                clone.Add(source[i].Clone());
        }
        return clone;
    }

    static List<GeneDataRecord> CloneRecords(List<GeneDataRecord> source)
    {
        List<GeneDataRecord> clone = new List<GeneDataRecord>();
        if (source == null)
            return clone;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
                clone.Add(source[i].Clone());
        }

        return clone;
    }

    static int GetPhaseRank(category value)
    {
        switch (value)
        {
            case category.grass:
                return 1;
            case category.herbivore:
                return 2;
            case category.predator:
                return 3;
            case category.highpredator:
                return 4;
            case category.dominant:
                return 5;
            default:
                return 0;
        }
    }
}
