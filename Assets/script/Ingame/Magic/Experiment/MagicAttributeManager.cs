using System;
using UnityEngine;

public class MagicAttributeManager : MonoBehaviour
{
    [Serializable]
    public struct AttributeDefinition
    {
        public MagicElement element;
        public Color projectileColor;
        public float projectileSpeed;
        public float projectileLifetime;
        public float effectRadius;
        public float iceSpikeHeight;
        public float iceSpikeRadius;
        public bool wrapNonTerrainTargets;
        public float envelopeLifetime;
        public float envelopePadding;
    }

    public AttributeDefinition[] definitions =
    {
        new AttributeDefinition
        {
            element = MagicElement.Fire,
            projectileColor = new Color(1f, 0.35f, 0.08f, 0.85f),
            projectileSpeed = 55f,
            projectileLifetime = 8f,
            effectRadius = 3f,
            iceSpikeHeight = 0f,
            iceSpikeRadius = 0f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 5f,
            envelopePadding = 0.2f
        },
        new AttributeDefinition
        {
            element = MagicElement.Ice,
            projectileColor = new Color(0.55f, 0.9f, 1f, 0.8f),
            projectileSpeed = 60f,
            projectileLifetime = 8f,
            effectRadius = 2f,
            iceSpikeHeight = 3f,
            iceSpikeRadius = 0.6f,
            wrapNonTerrainTargets = true,
            envelopeLifetime = 6f,
            envelopePadding = 0.25f
        },
        new AttributeDefinition
        {
            element = MagicElement.Lightning,
            projectileColor = new Color(1f, 0.95f, 0.25f, 0.9f),
            projectileSpeed = 120f,
            projectileLifetime = 4f,
            effectRadius = 1.5f,
            iceSpikeHeight = 0f,
            iceSpikeRadius = 0f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 3f,
            envelopePadding = 0.15f
        },
        new AttributeDefinition
        {
            element = MagicElement.Wind,
            projectileColor = new Color(0.65f, 1f, 0.75f, 0.45f),
            projectileSpeed = 75f,
            projectileLifetime = 6f,
            effectRadius = 4f,
            iceSpikeHeight = 0f,
            iceSpikeRadius = 0f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 4f,
            envelopePadding = 0.3f
        },
        new AttributeDefinition
        {
            element = MagicElement.Space,
            projectileColor = new Color(0.75f, 0.45f, 1f, 0.7f),
            projectileSpeed = 50f,
            projectileLifetime = 7f,
            effectRadius = 2.5f,
            iceSpikeHeight = 0f,
            iceSpikeRadius = 0f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 5f,
            envelopePadding = 0.25f
        }
    };

    public bool TryGetDefinition(MagicElement element, out AttributeDefinition definition)
    {
        for (int i = 0; i < definitions.Length; i++)
        {
            if (definitions[i].element == element)
            {
                definition = definitions[i];
                return true;
            }
        }

        definition = GetFallbackDefinition(element);
        return false;
    }

    public AttributeDefinition GetDefinition(MagicElement element)
    {
        TryGetDefinition(element, out AttributeDefinition definition);
        return definition;
    }

    static AttributeDefinition GetFallbackDefinition(MagicElement element)
    {
        return new AttributeDefinition
        {
            element = element,
            projectileColor = Color.white,
            projectileSpeed = 60f,
            projectileLifetime = 8f,
            effectRadius = 2f,
            iceSpikeHeight = 3f,
            iceSpikeRadius = 0.6f,
            wrapNonTerrainTargets = false,
            envelopeLifetime = 5f,
            envelopePadding = 0.2f
        };
    }
}
