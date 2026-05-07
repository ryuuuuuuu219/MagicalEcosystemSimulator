using UnityEngine;

public class ManaFieldAttractionDesire : MonoBehaviour, IAIDesire
{
    public float level = 1f;
    public float sampleOffset = 2f;

    public AIMoveIntent Evaluate(AIContext context)
    {
        if (context == null || context.Transform == null)
            return AIMoveIntent.None("mana field missing");

        ManaFieldManager field = ManaFieldManager.GetOrCreate();
        Vector3 position = context.Transform.position;
        float center = field.SampleMana(position);
        Vector3 gradient = Vector3.zero;
        gradient.x = field.SampleMana(position + Vector3.right * sampleOffset) - center;
        gradient.z = field.SampleMana(position + Vector3.forward * sampleOffset) - center;

        return new AIMoveIntent { direction = gradient, weight = level, reason = "mana field" };
    }
}
