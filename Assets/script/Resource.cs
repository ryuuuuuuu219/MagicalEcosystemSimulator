using UnityEngine;

public class Resource : MonoBehaviour
{
    public float carbon;
    public float maxCarbon;

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

        float eatable = Mathf.Min(amount, tgtresource.carbon);
        float receivable = Mathf.Min(eatable, maxCarbon - carbon);

        carbon += receivable;
        tgtresource.carbon -= receivable;
    }

    public void AddCarbon(float amount, out float excess)
    {
        float newCarbon = carbon + amount;
        excess = newCarbon > maxCarbon ? newCarbon - maxCarbon : 0;
        carbon = Mathf.Min(newCarbon, maxCarbon);
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

        float released = RemoveCarbon(Mathf.Max(0f, ratePerSec) * Time.deltaTime);
        if (released > 0f && dispenser != null)
            dispenser.ReturnCarbon(released);

        if (IsEmpty())
            Destroy(gameObject);
    }
    public void InitCarbon(float amount)
    {
        InitCarbon(amount, amount);
    }

    public void InitCarbon(float currentAmount, float maxAmount)
    {
        maxCarbon = Mathf.Max(0f, maxAmount);
        carbon = Mathf.Clamp(currentAmount, 0f, maxCarbon);
    }
}
