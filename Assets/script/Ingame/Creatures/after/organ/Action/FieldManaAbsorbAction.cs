using UnityEngine;

public class FieldManaAbsorbAction : MonoBehaviour, IAIAction
{
    public float manaAbsorbPerSec = 1f;
    public float absorbRadius = 2f;
    public bool convertFieldAbsorb;
    public float logScale = 1f;
    float nextAbsorbTime;

    public bool TryAct(AIContext context, float deltaTime)
    {
        if (context == null || context.BodyResource == null || Time.time < nextAbsorbTime)
            return false;

        nextAbsorbTime = Time.time + 1f;
        context.BodyResource.AbsorbManaFromField(manaAbsorbPerSec, absorbRadius, convertFieldAbsorb, logScale);
        return true;
    }
}
