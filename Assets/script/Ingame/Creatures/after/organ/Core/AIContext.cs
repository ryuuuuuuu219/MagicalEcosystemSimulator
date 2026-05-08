using UnityEngine;

public class AIContext
{
    public GameObject Self;
    public Transform Transform;
    public Terrain Terrain;
    public Resource BodyResource;
    public ResourceDispenser ResourceDispenser;
    public AIMemoryStore Memory;
    public CreatureRelationResolver RelationResolver;
    public Vector3 CurrentVelocity;
    public float Health;
    public float Mana;
    public float MaxMana;

    public static AIContext From(GameObject self)
    {
        AIContext context = new AIContext();
        context.Refresh(self);
        return context;
    }

    public void Refresh(GameObject self)
    {
        Self = self;
        Transform = self != null ? self.transform : null;
        if (self == null) return;

        BodyResource = self.GetComponent<Resource>();
        Memory = self.GetComponent<AIMemoryStore>();
        RelationResolver = self.GetComponent<CreatureRelationResolver>();
        ResourceDispenser = ResourceDispenser.Instance != null
            ? ResourceDispenser.Instance
            : Object.FindFirstObjectByType<ResourceDispenser>();

        if (self.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            Terrain = herbivore.terrain;
            Health = herbivore.health;
            Mana = herbivore.mana;
            MaxMana = herbivore.maxMana;
            CurrentVelocity = self.TryGetComponent<GroundMotor>(out var herbivoreMotor)
                ? herbivoreMotor.CurrentVelocity
                : herbivore.CurrentVelocity;
            return;
        }

        if (self.TryGetComponent<predatorBehaviour>(out var predator))
        {
            Terrain = predator.terrain;
            Health = predator.health;
            Mana = predator.mana;
            MaxMana = predator.maxMana;
            CurrentVelocity = self.TryGetComponent<GroundMotor>(out var predatorMotor)
                ? predatorMotor.CurrentVelocity
                : predator.CurrentVelocity;
            return;
        }

        if (BodyResource != null)
        {
            Mana = BodyResource.mana;
            MaxMana = BodyResource.maxMana;
        }
    }
}
