using UnityEngine;

public class Resource : MonoBehaviour
{
    public float mana;
    public float maxMana;
    public float totalReleasedMana;
    [TextArea(3, 10)]
    public string manaLog;
    public float corpseMaxLifetime;
    public float manaDecayAfterDeathPerSec;

    public category resourceCategory;
    public int speciesID;
    bool corpseDecayInitialized;
    bool isGenerationResetDisposal;

    public float Eating(float amount, Resource targetResource, string label = "eat")
    {
        if (targetResource == null) return 0f;

        float requestedAmount = Mathf.Max(0f, amount);
        float eatable = Mathf.Min(requestedAmount, targetResource.mana);
        if (eatable <= 0f) return 0f;

        mana += eatable;
        targetResource.mana -= eatable;
        RecordManaEvent(label + " gain", eatable);
        targetResource.RecordManaEvent(label + " loss", -eatable);
        return eatable;
    }

    public void AddMana(float amount, out float excess, string label = "add")
    {
        float addAmount = Mathf.Max(0f, amount);
        if (addAmount <= 0f)
        {
            excess = 0f;
            return;
        }

        mana += addAmount;
        excess = 0f;
        RecordManaEvent(label, addAmount);
    }

    public float RemoveMana(float amount, string label = "remove")
    {
        float removed = Mathf.Min(Mathf.Max(0f, amount), mana);
        mana -= removed;
        if (removed > 0f)
            RecordManaEvent(label, -removed);
        return removed;
    }

    public float AbsorbManaFromField(float requestedAmount, float radius, bool useLogConvert, float logScale)
    {
        float request = Mathf.Max(0f, requestedAmount);
        if (request <= 0f)
            return 0f;

        ManaFieldManager field = ManaFieldManager.GetOrCreate();
        float available = field.SampleMana(transform.position, Mathf.Max(0.1f, radius));
        float rawAmount = Mathf.Min(request, available);
        if (rawAmount <= 0f)
            return 0f;

        float absorbed = useLogConvert
            ? Mathf.Min(rawAmount, Mathf.Log(1f + rawAmount) * Mathf.Max(0.0001f, logScale))
            : rawAmount;

        if (absorbed <= 0f || !field.TrySpendMana(transform.position, absorbed, Mathf.Max(0.1f, radius)))
            return 0f;

        mana += absorbed;
        RecordManaEvent("field absorb", absorbed);
        return absorbed;
    }

    public bool IsEmpty()
    {
        return mana <= 0.0001f;
    }

    public void Decompose(float ratePerSec, ResourceDispenser dispenser)
    {
        if (isGenerationResetDisposal)
        {
            Destroy(gameObject);
            return;
        }

        if (TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = Vector3.zero;

        EnsureCorpseDecay(ratePerSec);

        float released = ReleaseManaToEnvironment(Mathf.Max(0f, manaDecayAfterDeathPerSec) * Time.deltaTime, dispenser);
        if (released > 0f)
        {
            ManaFieldManager.GetOrCreate().AddMana(transform.position, released, 3f);
        }

        if (IsEmpty())
            Destroy(gameObject);
    }

    public float ReleaseManaToEnvironment(float amount, ResourceDispenser dispenser)
    {
        float released = RemoveMana(amount, "field release");
        if (released <= 0f)
            return 0f;

        totalReleasedMana += released;
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
        manaLog = string.Empty;
        corpseMaxLifetime = 0f;
        manaDecayAfterDeathPerSec = 0f;
        corpseDecayInitialized = false;
        isGenerationResetDisposal = false;
    }

    public void MarkGenerationResetDisposal()
    {
        isGenerationResetDisposal = true;
        mana = 0f;
        RecordManaEvent("generation reset dispose", 0f);
    }

    void EnsureCorpseDecay(float fallbackRatePerSec)
    {
        if (corpseDecayInitialized)
            return;

        corpseDecayInitialized = true;
        float deathMana = Mathf.Max(0f, mana);
        if (deathMana <= 0f)
        {
            corpseMaxLifetime = 0f;
            manaDecayAfterDeathPerSec = Mathf.Max(0f, fallbackRatePerSec);
            return;
        }

        corpseMaxLifetime = Mathf.Max(1f, Mathf.Sqrt(10f * deathMana));
        manaDecayAfterDeathPerSec = deathMana / corpseMaxLifetime;
        RecordManaEvent("corpse decay init lifetime=" + corpseMaxLifetime.ToString("0.##"), 0f);
    }

    public void RecordManaEvent(string label, float amount)
    {
        if (Mathf.Abs(amount) <= 0.0001f && string.IsNullOrWhiteSpace(label))
            return;

        manaLog += $"{Time.time:F1}s {label} {amount:+0.###;-0.###} => {mana:0.###}\n";
        const int maxLength = 4000;
        if (manaLog.Length > maxLength)
            manaLog = manaLog.Substring(manaLog.Length - maxLength);
    }

    public string GetManaLogTail(int maxChars = 600)
    {
        if (string.IsNullOrEmpty(manaLog))
            return string.Empty;

        int safeChars = Mathf.Max(1, maxChars);
        if (manaLog.Length <= safeChars)
            return manaLog.TrimEnd();

        return manaLog.Substring(manaLog.Length - safeChars).TrimEnd();
    }
}
