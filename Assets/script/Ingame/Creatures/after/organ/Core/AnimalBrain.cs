using System.Collections.Generic;
using UnityEngine;

public interface IAISense
{
    void TickSense(AIContext context, float deltaTime);
}

public interface IAIDesire
{
    AIMoveIntent Evaluate(AIContext context);
}

public interface IAISteering
{
    Vector3 Steer(AIContext context, Vector3 desiredVector);
}

public interface IAIAction
{
    bool TryAct(AIContext context, float deltaTime);
}

public class AnimalBrain : MonoBehaviour
{
    readonly List<IAISense> senses = new();
    readonly List<IAIDesire> desires = new();
    readonly List<IAISteering> steerings = new();
    readonly List<IAIAction> actions = new();

    public AIContext Context { get; private set; }
    public Vector3 LastMoveVector { get; private set; }
    GroundMotor groundMotor;
    OrganFoundation foundation;

    void Awake()
    {
        RefreshOrgans();
        Context = AIContext.From(gameObject);
        groundMotor = GetComponent<GroundMotor>();
        foundation = GetComponent<OrganFoundation>();
    }

    public void RefreshOrgans()
    {
        senses.Clear();
        desires.Clear();
        steerings.Clear();
        actions.Clear();
        groundMotor = GetComponent<GroundMotor>();

        senses.AddRange(GetComponents<IAISense>());
        desires.AddRange(GetComponents<IAIDesire>());
        steerings.AddRange(GetComponents<IAISteering>());
        actions.AddRange(GetComponents<IAIAction>());
    }

    public Vector3 TickBrain(float deltaTime)
    {
        Context.Refresh(gameObject);
        if (Context.IsDead)
        {
            LastMoveVector = Vector3.zero;
            if (groundMotor == null)
                groundMotor = GetComponent<GroundMotor>();
            if (groundMotor != null)
                groundMotor.Stop();
            return LastMoveVector;
        }

        for (int i = 0; i < senses.Count; i++)
        {
            if (ShouldRunOrgan(senses[i]))
                senses[i].TickSense(Context, deltaTime);
        }

        for (int i = 0; i < actions.Count; i++)
        {
            if (ShouldRunOrgan(actions[i]))
                actions[i].TryAct(Context, deltaTime);
        }

        Vector3 total = Vector3.zero;
        for (int i = 0; i < desires.Count; i++)
        {
            if (!ShouldRunOrgan(desires[i]))
                continue;

            AIMoveIntent intent = desires[i].Evaluate(Context);
            total += intent.Vector;
        }

        for (int i = 0; i < steerings.Count; i++)
        {
            if (ShouldRunOrgan(steerings[i]))
                total = steerings[i].Steer(Context, total);
        }

        LastMoveVector = total;
        if (groundMotor == null)
            groundMotor = GetComponent<GroundMotor>();
        if (groundMotor != null)
            groundMotor.Move(Context, LastMoveVector, deltaTime);

        return LastMoveVector;
    }

    bool ShouldRunOrgan(object organ)
    {
        if (foundation == null)
            foundation = GetComponent<OrganFoundation>();
        if (foundation == null || organ is not Component component)
            return true;
        return foundation.IsOrganActive(component);
    }
}
