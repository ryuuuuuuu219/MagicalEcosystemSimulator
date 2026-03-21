using UnityEngine;

public class Resource : MonoBehaviour
{
    public float carbon;
    public float maxCarbon;
    public float totalReleasedCarbon;

    public enum category
    {
        grass,
        herbivore,
        predator
    }
    public category resourceCategory;

    public void Eating(float amount, Resource tgtresource)
    {
        if (tgtresource == null) return;

        float requestedAmount = Mathf.Max(0f, amount) * GetAbsorptionScale();
        float eatable = Mathf.Min(requestedAmount, tgtresource.carbon);
        if (eatable <= 0f) return;

        carbon += eatable;
        tgtresource.carbon -= eatable;
    }

    public void AddCarbon(float amount, out float excess)
    {
        float addAmount = Mathf.Max(0f, amount);
        if (addAmount <= 0f)
        {
            excess = 0f;
            return;
        }

        if (maxCarbon <= 0f)
        {
            carbon += addAmount;
            excess = 0f;
            return;
        }

        float remainingCapacity = Mathf.Max(0f, maxCarbon - carbon);
        float accepted = Mathf.Min(addAmount, remainingCapacity);
        carbon += accepted;
        excess = addAmount - accepted;
    }

    public float RemoveCarbon(float amount)
    {
        float removed = Mathf.Min(amount, carbon);
        carbon -= removed;
        return removed;
    }

    public bool IsEmpty()
    {
        return carbon <= 0.0001f;
    }

    public void Decompose(float ratePerSec, ResourceDispenser dispenser)
    {
        if (TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = Vector3.zero;

        float released = ReleaseCarbonToEnvironment(Mathf.Max(0f, ratePerSec) * Time.deltaTime, dispenser);
        if (released > 0f)
        {
            HeatFieldManager heatField = HeatFieldManager.GetOrCreate();
            float heatAmount = released * (dispenser != null ? dispenser.decompositionHeatPerCarbon : 1f);
            heatField.AddHeat(transform.position, heatAmount, 3f);
        }

        if (IsEmpty())
            Destroy(gameObject);
    }

    public float ReleaseCarbonToEnvironment(float amount, ResourceDispenser dispenser)
    {
        float released = RemoveCarbon(amount);
        if (released <= 0f)
            return 0f;

        totalReleasedCarbon += released;
        if (dispenser != null)
            dispenser.ReturnCarbon(released);
        return released;
    }

    public float ReleaseAllCarbonToEnvironment(ResourceDispenser dispenser)
    {
        return ReleaseCarbonToEnvironment(carbon, dispenser);
    }

    public float GetCurrentEnergyAmount()
    {
        if (TryGetComponent<herbivoreBehaviour>(out var herbivore))
            return Mathf.Max(0f, herbivore.energy);
        if (TryGetComponent<predatorBehaviour>(out var predator))
            return Mathf.Max(0f, predator.energy);
        return 0f;
    }

    public float CalculateDynamicMass()
    {
        return Mathf.Max(0.001f, carbon + GetCurrentEnergyAmount());
    }

    public void InitCarbon(float amount)
    {
        InitCarbon(amount, amount);
    }

    public void InitCarbon(float currentAmount, float maxAmount)
    {
        maxCarbon = Mathf.Max(0f, maxAmount);
        carbon = Mathf.Max(0f, currentAmount);
        totalReleasedCarbon = 0f;
    }

    float GetAbsorptionScale()
    {
        if (maxCarbon <= 0f)
            return 1f;

        float ratio = carbon / maxCarbon;
        if (ratio <= 1f)
            return 1f;

        return 1f / ratio;
    }
}
