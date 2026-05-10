using UnityEngine;

public class MagicElementAffinityState : MonoBehaviour
{
    public bool fireUnlocked;
    public bool iceUnlocked;
    public bool lightningUnlocked;
    public bool windUnlocked;
    public int normalElementUnlockCount;
    public float mutationInterval = 10f;
    public float fieldUnlockCoefficient = 0.00001f;
    public float unlockProbabilityCap = 0.005f;
    float nextMutationTime;

    void Update()
    {
        TickFieldAssistedUnlock();
    }

    public int CountNormalElements()
    {
        int count = 0;
        if (fireUnlocked) count++;
        if (iceUnlocked) count++;
        if (lightningUnlocked) count++;
        if (windUnlocked) count++;
        normalElementUnlockCount = count;
        return count;
    }

    public bool HasElement(MagicElement element)
    {
        switch (element)
        {
            case MagicElement.Fire:
                return fireUnlocked;
            case MagicElement.Ice:
                return iceUnlocked;
            case MagicElement.Lightning:
                return lightningUnlocked;
            case MagicElement.Wind:
                return windUnlocked;
            default:
                return false;
        }
    }

    public MagicElement EnsureAtLeastOneNormalElement()
    {
        if (CountNormalElements() > 0)
            return GetPreferredNormalElement();

        MagicElement element = RandomNormalElement();
        Unlock(element);
        return element;
    }

    public void EnsureAtLeastNormalElementCount(int requiredCount)
    {
        int safeRequired = Mathf.Clamp(requiredCount, 0, 4);
        while (CountNormalElements() < safeRequired)
            Unlock(RandomLockedNormalElement());
    }

    public void Unlock(MagicElement element)
    {
        switch (element)
        {
            case MagicElement.Fire:
                fireUnlocked = true;
                break;
            case MagicElement.Ice:
                iceUnlocked = true;
                break;
            case MagicElement.Lightning:
                lightningUnlocked = true;
                break;
            case MagicElement.Wind:
                windUnlocked = true;
                break;
        }

        CountNormalElements();
    }

    public MagicElement GetPreferredNormalElement()
    {
        if (fireUnlocked) return MagicElement.Fire;
        if (iceUnlocked) return MagicElement.Ice;
        if (lightningUnlocked) return MagicElement.Lightning;
        if (windUnlocked) return MagicElement.Wind;
        return EnsureAtLeastOneNormalElement();
    }

    void TickFieldAssistedUnlock()
    {
        if (Time.time < nextMutationTime)
            return;

        nextMutationTime = Time.time + Mathf.Max(0.1f, mutationInterval);
        if (!TryGetComponent<Resource>(out var resource))
            return;
        if (resource.resourceCategory < category.predator)
            return;

        if (CountNormalElements() <= 0)
        {
            EnsureAtLeastOneNormalElement();
            return;
        }

        if (resource.resourceCategory != category.highpredator)
            return;
        if (CountNormalElements() >= 2)
            return;

        float fieldMana = ManaFieldManager.GetOrCreate().SampleMana(transform.position);
        float probability = Mathf.Min(
            Mathf.Max(0f, unlockProbabilityCap),
            Mathf.Max(0f, fieldMana) * Mathf.Max(0f, fieldUnlockCoefficient));
        if (Random.value <= probability)
            Unlock(RandomLockedNormalElement());
    }

    MagicElement RandomLockedNormalElement()
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            MagicElement element = RandomNormalElement();
            if (!HasElement(element))
                return element;
        }

        if (!fireUnlocked) return MagicElement.Fire;
        if (!iceUnlocked) return MagicElement.Ice;
        if (!lightningUnlocked) return MagicElement.Lightning;
        return MagicElement.Wind;
    }

    static MagicElement RandomNormalElement()
    {
        switch (Random.Range(0, 4))
        {
            case 0:
                return MagicElement.Fire;
            case 1:
                return MagicElement.Ice;
            case 2:
                return MagicElement.Lightning;
            default:
                return MagicElement.Wind;
        }
    }
}
