using UnityEngine;

public class PredatorPhaseEvolutionAction : MonoBehaviour, IAIAction
{
    public float phaseCheckInterval = 10f;
    public float phaseUpManaCoefficient = 0.00001f;
    public float phaseUpProbabilityCap = 0.005f;
    float nextPhaseCheckTime;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.BodyResource == null || context.IsDead)
            return false;

        EnsurePredatorPhase(context.BodyResource);
        if (Time.time < nextPhaseCheckTime)
            return false;

        nextPhaseCheckTime = Time.time + Mathf.Max(0.1f, phaseCheckInterval);

        int currentRank = GetPhaseRank(context.BodyResource.resourceCategory);
        if (currentRank < 3 || currentRank >= 5)
            return false;

        float fieldMana = ManaFieldManager.GetOrCreate().SampleMana(context.Transform.position);
        float probability = Mathf.Min(
            Mathf.Max(0f, phaseUpProbabilityCap),
            Mathf.Max(0f, fieldMana) * Mathf.Max(0f, phaseUpManaCoefficient));
        if (Random.value > probability)
            return false;

        context.BodyResource.resourceCategory = GetCategoryFromPhaseRank(currentRank + 1);
        context.BodyResource.speciesID = DrawPhaseUpSpeciesID();
        context.BodyResource.RecordManaEvent(
            "organ phase up " + context.BodyResource.resourceCategory + " speciesID=" + context.BodyResource.speciesID,
            0f);
        OrganPresetLibrary.EnsurePredator(gameObject, context.BodyResource.resourceCategory);
        GeneDataManager.ApplyToCreature(gameObject);
        return true;
    }

    static void EnsurePredatorPhase(Resource resource)
    {
        if (resource != null && GetPhaseRank(resource.resourceCategory) < 3)
            resource.resourceCategory = category.predator;
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

    static category GetCategoryFromPhaseRank(int rank)
    {
        switch (rank)
        {
            case 1:
                return category.grass;
            case 2:
                return category.herbivore;
            case 3:
                return category.predator;
            case 4:
                return category.highpredator;
            case 5:
                return category.dominant;
            default:
                return category.predator;
        }
    }

    static int DrawPhaseUpSpeciesID()
    {
        int maxSpeciesID = 0;
        Resource[] resources = FindObjectsByType<Resource>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i] == null) continue;
            maxSpeciesID = Mathf.Max(maxSpeciesID, resources[i].speciesID);
        }

        int candidateMax = maxSpeciesID + 1;
        float totalWeight = 0f;
        for (int id = 0; id <= candidateMax; id++)
            totalWeight += 1f / (id + 1f);

        float roll = Random.value * totalWeight;
        for (int id = 0; id <= candidateMax; id++)
        {
            roll -= 1f / (id + 1f);
            if (roll <= 0f)
                return id;
        }

        return candidateMax;
    }
}
