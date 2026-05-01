using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class MagicProjectile : MonoBehaviour
{
    public MagicElement element = MagicElement.Ice;
    public float lifeTime = 8f;
    public float effectRadius = 2f;
    public float iceSpikeHeight = 3f;
    public float iceSpikeRadius = 0.6f;
    public bool wrapNonTerrainTargets;
    public float effectLifetime = 6f;
    public float envelopeLifetime = 5f;
    public float envelopePadding = 0.2f;
    public Color impactMaterialColor = Color.white;
    public Vector3 launchPoint;
    public float projectileSpeed = 1f;

    bool hasImpacted;

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = 0.18f;
        col.isTrigger = false;

        var body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted)
            return;

        ContactPoint contact = collision.GetContact(0);
        ApplyImpact(contact.point, contact.normal, collision.collider);
    }

    public void ApplyImpact(Vector3 point, Vector3 normal, Collider target)
    {
        hasImpacted = true;

        if (wrapNonTerrainTargets && target.GetComponent<TerrainCollider>() == null)
        {
            MagicImpactEnvelopeEffect.Create(target, impactMaterialColor, envelopeLifetime, envelopePadding);
            Debug.Log($"{element} projectile wrapped target: target={target.name}, point={point}, radius={effectRadius}");
            Destroy(gameObject);
            return;
        }

        switch (element)
        {
            case MagicElement.Ice:
                IceImpactEffect.CreateSpike(point, normal, iceSpikeHeight, iceSpikeRadius, effectLifetime);
                Debug.Log($"Ice projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            case MagicElement.Lightning:
                LightningImpactEffect.Create(launchPoint, point, normal, effectRadius, projectileSpeed, effectLifetime);
                Debug.Log($"Lightning projectile impact: target={target.name}, point={point}, radius={effectRadius}, speed={projectileSpeed}");
                break;
            case MagicElement.Space:
                SpaceWarpImpactEffect.CreateWarp(point, normal, effectRadius, effectLifetime);
                Debug.Log($"Space projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            default:
                Debug.Log($"{element} projectile impact has no payload yet: target={target.name}, point={point}");
                break;
        }

        Destroy(gameObject);
    }
}
