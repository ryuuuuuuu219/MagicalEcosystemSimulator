using UnityEngine;

public class CreatureVirtualGauge : MonoBehaviour
{
    herbivoreBehaviour herbivore;
    predatorBehaviour predator;
    Resource bodyResource;

    [Header("Life")]
    public float maxHealth = 25f;
    public float health = 25f;

    [Header("Energy")]
    public float maxEnergy = 100f;
    public float energy = 100f;

    [Header("Carbon")]
    public float maxCarbon = 100f;
    public float carbon = 100f;

    public void Initialize(herbivoreBehaviour owner)
    {
        herbivore = owner;
        predator = null;
        SyncStats();
    }

    public void Initialize(predatorBehaviour owner)
    {
        predator = owner;
        herbivore = null;
        SyncStats();
    }

    public bool IsAliveTarget
    {
        get
        {
            if (herbivore != null)
                return herbivore.enabled;
            if (predator != null)
                return predator.enabled;
            return false;
        }
    }

    public Vector3 GetAnchorWorldPosition()
    {
        float heightOffset = 2.2f;

        if (TryGetComponent<Collider>(out var col))
            heightOffset = Mathf.Max(heightOffset, col.bounds.extents.y * 2f + 0.6f);
        else
        {
            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
                heightOffset = Mathf.Max(heightOffset, rend.bounds.extents.y * 2f + 0.6f);
        }

        return transform.position + Vector3.up * heightOffset;
    }

    public bool IsHerbivore => herbivore != null;
    public bool IsPredator => predator != null;

    public bool TryGetRatios(out float healthRatio, out float energyRatio, out float carbonRatio, out int energyLap)
    {
        SyncStats();

        bool hasOwner = herbivore != null || predator != null;
        healthRatio = maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
        if (maxEnergy > 0f)
        {
            float normalized = Mathf.Max(0f, energy) / maxEnergy;
            energyLap = Mathf.FloorToInt(normalized);
            energyRatio = normalized - energyLap;
            if (energyRatio <= 0f && normalized > 0f)
            {
                energyLap = Mathf.Max(0, energyLap - 1);
                energyRatio = 1f;
            }
        }
        else
        {
            energyLap = 0;
            energyRatio = 0f;
        }

        carbonRatio = maxCarbon > 0f ? Mathf.Clamp01(Mathf.Max(0f, carbon) / maxCarbon) : 0f;

        return hasOwner;
    }

    void SyncStats()
    {
        if (bodyResource == null)
            bodyResource = GetComponent<Resource>();

        if (herbivore != null)
        {
            maxHealth = herbivore.maxHealth;
            health = herbivore.health;
            maxEnergy = herbivore.maxEnergy;
            energy = herbivore.energy;
            if (bodyResource != null)
            {
                maxCarbon = bodyResource.maxCarbon;
                carbon = bodyResource.carbon;
            }
            return;
        }

        if (predator != null)
        {
            maxHealth = predator.maxHealth;
            health = predator.health;
            maxEnergy = predator.maxEnergy;
            energy = predator.energy;
            if (bodyResource != null)
            {
                maxCarbon = bodyResource.maxCarbon;
                carbon = bodyResource.carbon;
            }
        }
    }
}
