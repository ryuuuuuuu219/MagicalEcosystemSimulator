using UnityEngine;

public class Resource : MonoBehaviour
{
    public float mana;
    public float maxMana;
    public float totalReleasedMana;

    public category resourceCategory;

    public void Eating(float amount, Resource targetResource)
    {
        if (targetResource == null) return;

        float requestedAmount = Mathf.Max(0f, amount);
        float eatable = Mathf.Min(requestedAmount, targetResource.mana);
        if (eatable <= 0f) return;

        mana += eatable;
        targetResource.mana -= eatable;
    }

    public void AddMana(float amount, out float excess)
    {
        float addAmount = Mathf.Max(0f, amount);
        if (addAmount <= 0f)
        {
            excess = 0f;
            return;
        }

        mana += addAmount;
        excess = 0f;
    }

    public float RemoveMana(float amount)
    {
        float removed = Mathf.Min(Mathf.Max(0f, amount), mana);
        mana -= removed;
        return removed;
    }

    public bool IsEmpty()
    {
        return mana <= 0.0001f;
    }

    public void Decompose(float ratePerSec, ResourceDispenser dispenser)
    {
        if (TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = Vector3.zero;

        float released = ReleaseManaToEnvironment(Mathf.Max(0f, ratePerSec) * Time.deltaTime, dispenser);
        if (released > 0f)
        {
            ManaFieldManager.GetOrCreate().AddMana(transform.position, released, 3f);
        }

        if (IsEmpty())
            Destroy(gameObject);
    }

    public float ReleaseManaToEnvironment(float amount, ResourceDispenser dispenser)
    {
        float released = RemoveMana(amount);
        if (released <= 0f)
            return 0f;

        totalReleasedMana += released;
        if (dispenser != null)
            dispenser.ReturnMana(released);
        return released;
    }

    public float ReleaseAllManaToEnvironment(ResourceDispenser dispenser)
    {
        return ReleaseManaToEnvironment(mana, dispenser);
    }

    public float CalculateDynamicMass()
    {
        return Mathf.Max(0.001f, mana);
    }

    public void InitMana(float amount)
    {
        InitMana(amount, amount);
    }

    public void InitMana(float currentAmount, float maxAmount)
    {
        maxMana = Mathf.Max(0f, maxAmount);
        mana = Mathf.Max(0f, currentAmount);
        totalReleasedMana = 0f;
    }
}
