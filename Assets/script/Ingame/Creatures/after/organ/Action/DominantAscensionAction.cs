using UnityEngine;

public class DominantAscensionAction : MonoBehaviour, IAIAction
{
    public int requiredNormalElementCount = 2;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.BodyResource == null || context.IsDead)
            return false;
        if (context.BodyResource.resourceCategory != category.highpredator)
            return false;

        MagicElementAffinityState affinity = GetComponent<MagicElementAffinityState>();
        if (affinity == null || affinity.CountNormalElements() < Mathf.Max(1, requiredNormalElementCount))
            return false;

        context.BodyResource.resourceCategory = category.dominant;
        context.BodyResource.RecordManaEvent("dominant ascension normalMagic=" + affinity.CountNormalElements(), 0f);

        EnsureDominantOrgans();
        DominantLineageTracker.RecordNaturalDominant(context.BodyResource.speciesID);
        GeneDataManager.ApplyToCreature(gameObject);
        return true;
    }

    void EnsureDominantOrgans()
    {
        AnimalAIInstaller installer = GetComponent<AnimalAIInstaller>();
        if (installer == null)
            installer = gameObject.AddComponent<AnimalAIInstaller>();

        installer.Ensure<MagicAttackAction>();
        installer.Ensure<MagicProjectileAttackAction>();
        installer.Ensure<MagicCooldownState>();
        installer.Ensure<MagicElementAffinityState>();
        installer.Ensure<SpaceMagicAction>();

        installer.componentSet.ProtectGene(nameof(MagicAttackAction), true);
        installer.componentSet.ProtectGene(nameof(MagicProjectileAttackAction), true);
        installer.componentSet.ProtectGene(nameof(MagicCooldownState), true);
        installer.componentSet.ProtectGene(nameof(MagicElementAffinityState), true);
        installer.componentSet.ProtectGene(nameof(DominantAscensionAction), true);
        installer.componentSet.ProtectGene(nameof(SpaceMagicAction), true);

        MagicElementAffinityState affinity = GetComponent<MagicElementAffinityState>();
        if (affinity != null)
            affinity.EnsureAtLeastNormalElementCount(requiredNormalElementCount);

        SpaceMagicAction.Ensure(gameObject);

        if (TryGetComponent<OrganFoundation>(out var foundation))
        {
            foundation.RefreshInstalledOrganList();
            foundation.RecordCheckpoint("dominant ascension");
        }
        if (TryGetComponent<AnimalBrain>(out var brain))
            brain.RefreshOrgans();
    }
}
