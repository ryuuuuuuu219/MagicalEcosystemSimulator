using UnityEngine;

public class CreatureVirtualGauge : MonoBehaviour
{
    herbivoreBehaviour herbivore;
    predatorBehaviour predator;

    public void Initialize(herbivoreBehaviour owner)
    {
        herbivore = owner;
        predator = null;
    }

    public void Initialize(predatorBehaviour owner)
    {
        predator = owner;
        herbivore = null;
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

    public bool TryGetRatios(out float healthRatio, out float energyRatio)
    {
        if (herbivore != null)
        {
            healthRatio = herbivore.maxHealth > 0f ? Mathf.Clamp01(herbivore.health / herbivore.maxHealth) : 0f;
            energyRatio = herbivore.maxEnergy > 0f ? Mathf.Clamp01(herbivore.energy / herbivore.maxEnergy) : 0f;
            return true;
        }

        if (predator != null)
        {
            healthRatio = predator.maxHealth > 0f ? Mathf.Clamp01(predator.health / predator.maxHealth) : 0f;
            energyRatio = predator.maxEnergy > 0f ? Mathf.Clamp01(predator.energy / predator.maxEnergy) : 0f;
            return true;
        }

        healthRatio = 0f;
        energyRatio = 0f;
        return false;
    }
}
