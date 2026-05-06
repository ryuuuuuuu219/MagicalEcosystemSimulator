using UnityEngine;

public class CreatureVirtualGauge : MonoBehaviour
{
    herbivoreBehaviour herbivore;
    predatorBehaviour predator;
    Resource bodyResource;

    [Header("Life")]
    public float maxHealth = 25f;
    public float health = 25f;

    [Header("Mana")]
    public float maxMana = 100f;
    public float mana = 100f;

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

    public bool TryGetRatios(out float healthRatio, out float manaRatio, out float resourceManaRatio, out int manaLap)
    {
        SyncStats();

        bool hasOwner = herbivore != null || predator != null;
        healthRatio = maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
        if (maxMana > 0f)
        {
            float normalized = Mathf.Max(0f, mana) / maxMana;
            manaLap = Mathf.FloorToInt(normalized);
            manaRatio = normalized - manaLap;
            if (manaRatio <= 0f && normalized > 0f)
            {
                manaLap = Mathf.Max(0, manaLap - 1);
                manaRatio = 1f;
            }
        }
        else
        {
            manaLap = 0;
            manaRatio = 0f;
        }

        resourceManaRatio = maxMana > 0f ? Mathf.Clamp01(Mathf.Max(0f, mana) / maxMana) : 0f;

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
            maxMana = herbivore.maxMana;
            mana = herbivore.mana;
            if (bodyResource != null)
            {
                maxMana = bodyResource.maxMana;
                mana = bodyResource.mana;
            }
            return;
        }

        if (predator != null)
        {
            maxHealth = predator.maxHealth;
            health = predator.health;
            maxMana = predator.maxMana;
            mana = predator.mana;
            if (bodyResource != null)
            {
                maxMana = bodyResource.maxMana;
                mana = bodyResource.mana;
            }
        }
    }
}
