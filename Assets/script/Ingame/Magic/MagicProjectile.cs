using System.Collections.Generic;
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
    public bool logExpiredWithoutImpact;
    public Resource casterResource;
    public float magicDamage = 12f;
    public float magicDamageToManaRate = 1f;
    public float magicRecoveryWindow = 8f;
    public int magicTargetCount = 1;
    public float magicMaxNetGainPerCast;

    bool hasImpacted;
    bool isFinishing;
    Rigidbody body;
    Vector3 intendedVelocity;
    float launchTime;

    void Awake()
    {
        var col = GetComponent<SphereCollider>();
        col.radius = 0.18f;
        col.isTrigger = false;

        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        launchTime = Time.time;
        if (intendedVelocity.sqrMagnitude <= 0.0001f && body != null)
            intendedVelocity = body.linearVelocity;

        Invoke(nameof(ExpireWithoutImpact), Mathf.Max(0.01f, lifeTime));
    }

    public void SetIntendedVelocity(Vector3 velocity)
    {
        intendedVelocity = velocity;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted)
            return;

        Collider target = collision.collider;
        if (IsPassThroughCollider(target))
        {
            IgnoreCollisionWith(target);
            RestoreIntendedVelocity();
            return;
        }

        ContactPoint contact = collision.GetContact(0);
        ApplyImpact(contact.point, contact.normal, target);
    }

    public void ApplyImpact(Vector3 point, Vector3 normal, Collider target)
    {
        if (isFinishing)
            return;

        if (IsDestroyWithoutImpactCollider(target))
        {
            DestroyWithoutImpact($"hit non-impact environment collider: target={target.name}, point={point}");
            return;
        }

        if (!ShouldExplodeOnImpact(target))
        {
            string targetName = target != null ? target.name : "null";
            DestroyWithoutImpact($"hit non-impact target: target={targetName}, point={point}");
            return;
        }

        BeginImpactFinish();
        CreateParticleImpact(point, normal);
        Magic2FieldManager.Apply(element, point, normal, effectRadius, effectLifetime);
        ApplyMagicDamage(point, target);

        if (wrapNonTerrainTargets && target.GetComponent<TerrainCollider>() == null)
        {
            MagicImpactEnvelopeEffect.Create(target, impactMaterialColor, envelopeLifetime, envelopePadding);
            Debug.Log($"{element} projectile wrapped target: target={target.name}, point={point}, radius={effectRadius}");
            DestroyProjectile();
            return;
        }

        switch (element)
        {
            case MagicElement.Fire:
                Debug.Log($"Fire projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            case MagicElement.Ice:
                IceImpactEffect.CreateSpike(point, normal, iceSpikeHeight, iceSpikeRadius, effectLifetime);
                Debug.Log($"Ice projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            case MagicElement.Lightning:
                LightningImpactEffect.Create(launchPoint, point, normal, effectRadius, projectileSpeed, effectLifetime);
                Debug.Log($"Lightning projectile impact: target={target.name}, point={point}, radius={effectRadius}, speed={projectileSpeed}");
                break;
            case MagicElement.Wind:
                WindImpactEffect.Create(point, normal, effectRadius, effectLifetime);
                Debug.Log($"Wind projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            case MagicElement.Space:
                SpaceWarpImpactEffect.CreateWarp(point, normal, effectRadius, effectLifetime);
                Debug.Log($"Space projectile impact: target={target.name}, point={point}, radius={effectRadius}");
                break;
            default:
                Debug.Log($"{element} projectile impact has no payload yet: target={target.name}, point={point}");
                break;
        }

        DestroyProjectile();
    }

    void ApplyMagicDamage(Vector3 point, Collider directTarget)
    {
        float damage = Mathf.Max(0f, magicDamage);
        if (damage <= 0f)
            return;

        int remainingTargets = Mathf.Max(1, magicTargetCount);
        float totalDamage = 0f;
        HashSet<GameObject> damagedObjects = new HashSet<GameObject>();

        totalDamage += TryDamageColliderTarget(directTarget, damage, damagedObjects, ref remainingTargets);

        if (remainingTargets > 0 && effectRadius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(point, effectRadius);
            for (int i = 0; i < hits.Length && remainingTargets > 0; i++)
                totalDamage += TryDamageColliderTarget(hits[i], damage, damagedObjects, ref remainingTargets);
        }

        TryRecoverManaFromMagicDamage(totalDamage);
    }

    float TryDamageColliderTarget(Collider target, float damage, HashSet<GameObject> damagedObjects, ref int remainingTargets)
    {
        if (target == null || remainingTargets <= 0)
            return 0f;

        herbivoreBehaviour herbivore = target.GetComponentInParent<herbivoreBehaviour>();
        if (herbivore != null)
        {
            GameObject key = herbivore.gameObject;
            if (!damagedObjects.Add(key) || herbivore.IsDead)
                return 0f;

            float before = herbivore.health;
            herbivore.TakeDamage(damage);
            remainingTargets--;
            return Mathf.Max(0f, before - herbivore.health);
        }

        predatorBehaviour predator = target.GetComponentInParent<predatorBehaviour>();
        if (predator != null)
        {
            GameObject key = predator.gameObject;
            if (!damagedObjects.Add(key) || predator.IsDead)
                return 0f;

            float before = predator.health;
            predator.TakeDamage(damage);
            remainingTargets--;
            return Mathf.Max(0f, before - predator.health);
        }

        return 0f;
    }

    void TryRecoverManaFromMagicDamage(float totalDamage)
    {
        if (casterResource == null || totalDamage <= 0f)
            return;
        if (magicRecoveryWindow > 0f && Time.time - launchTime > magicRecoveryWindow)
            return;

        float gain = totalDamage * Mathf.Max(0f, magicDamageToManaRate);
        if (magicMaxNetGainPerCast > 0f)
            gain = Mathf.Min(gain, magicMaxNetGainPerCast);
        if (gain <= 0f)
            return;

        casterResource.AddMana(gain, out _, "magic damage recover");
    }

    void ExpireWithoutImpact()
    {
        if (isFinishing || hasImpacted)
            return;

        isFinishing = true;
        if (logExpiredWithoutImpact)
            Debug.Log($"{element} projectile expired without impact: position={transform.position}, lifetime={lifeTime}");

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (!isFinishing && !hasImpacted)
            HandleExternalDestroyWithoutImpact();
    }

    void BeginImpactFinish()
    {
        isFinishing = true;
        hasImpacted = true;
        CancelInvoke(nameof(ExpireWithoutImpact));
    }

    void DestroyProjectile()
    {
        CancelInvoke(nameof(ExpireWithoutImpact));
        Destroy(gameObject);
    }

    void HandleExternalDestroyWithoutImpact()
    {
        if (logExpiredWithoutImpact)
            Debug.Log($"{element} projectile destroyed without impact: position={transform.position}");
    }

    void DestroyWithoutImpact(string reason)
    {
        isFinishing = true;
        CancelInvoke(nameof(ExpireWithoutImpact));
        if (logExpiredWithoutImpact)
            Debug.Log($"{element} projectile ended without impact effect: {reason}");

        Destroy(gameObject);
    }

    void IgnoreCollisionWith(Collider target)
    {
        if (target == null)
            return;

        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
            Physics.IgnoreCollision(projectileCollider, target, true);
    }

    void RestoreIntendedVelocity()
    {
        if (body == null || intendedVelocity.sqrMagnitude <= 0.0001f)
            return;

        body.linearVelocity = intendedVelocity;
    }

    static bool ShouldExplodeOnImpact(Collider target)
    {
        return IsTerrainCollider(target) || IsCreatureCollider(target);
    }

    static bool IsTerrainCollider(Collider target)
    {
        return target != null && target.GetComponent<TerrainCollider>() != null;
    }

    static bool IsCreatureCollider(Collider target)
    {
        if (target == null)
            return false;

        if (target.GetComponentInParent<CreatureCollisionProfile>() != null)
            return true;
        if (target.GetComponentInParent<herbivoreBehaviour>() != null)
            return true;
        if (target.GetComponentInParent<predatorBehaviour>() != null)
            return true;

        int creatureLayer = LayerMask.NameToLayer("Creature");
        if (creatureLayer < 0)
            return false;

        Transform current = target.transform;
        while (current != null)
        {
            if (current.gameObject.layer == creatureLayer)
                return true;

            current = current.parent;
        }

        return false;
    }

    static bool IsPassThroughCollider(Collider target)
    {
        if (target == null)
            return false;

        if (target.GetComponent<WorldWaterCollider>() != null)
            return true;

        return target.name == "Water" || target.gameObject.layer == LayerMask.NameToLayer("Water");
    }

    static bool IsDestroyWithoutImpactCollider(Collider target)
    {
        if (target == null)
            return false;

        if (target.GetComponent<WorldBoundaryCollider>() != null)
            return true;

        return target.name.StartsWith("InvisibleWall_");
    }

    void CreateParticleImpact(Vector3 point, Vector3 normal)
    {
        GameObject particleObject = new GameObject($"{element} Particle Impact");
        Vector3 impactNormal = normal.sqrMagnitude > 0.001f ? normal.normalized : Vector3.up;
        particleObject.transform.position = point + impactNormal * 0.05f;
        particleObject.transform.up = impactNormal;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        if (element == MagicElement.Fire)
            ConfigureFireParticles(particles);
        else
            ConfigureShockwaveParticles(particles);

        particles.Play(true);
        Destroy(particleObject, Mathf.Max(1.5f, effectLifetime));
    }

    void ConfigureFireParticles(ParticleSystem particles)
    {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.loop = false;
        main.duration = 1.1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.55f, 1.25f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 6.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.95f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.12f, 0.02f, 1f), new Color(1f, 0.95f, 0.12f, 1f));
        main.gravityModifier = -0.85f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 180;

        var emission = particles.emission;
        emission.enabled = true;
        int burstCount = Mathf.Clamp(Mathf.RoundToInt(effectRadius * 34f), 45, 130);
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)burstCount)
        });

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = Mathf.Max(0.15f, effectRadius * 0.28f);
        shape.length = Mathf.Max(0.2f, effectRadius * 0.45f);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.95f, 0.18f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0.03f), 0.45f),
                new GradientColorKey(new Color(0.12f, 0.02f, 0.01f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.65f, 0.55f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial(new Color(1f, 0.45f, 0.05f, 1f));
        particles.Emit(burstCount);
    }

    void ConfigureShockwaveParticles(ParticleSystem particles)
    {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.loop = false;
        main.duration = 0.55f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.9f), new Color(0.75f, 0.9f, 1f, 0.55f));
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 140;

        var emission = particles.emission;
        emission.enabled = true;
        int burstCount = Mathf.Clamp(Mathf.RoundToInt(effectRadius * 28f), 28, 95);
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)burstCount)
        });

        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = Mathf.Max(0.2f, effectRadius * 0.45f);

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.8f, 0.92f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.85f, 0f),
                new GradientAlphaKey(0.25f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateParticleMaterial(Color.white);
        particles.Emit(burstCount);
    }

    static Material CreateParticleMaterial(Color color)
    {
        Shader shader = Shader.Find("MagicalEcosystem/Experiment/MagicParticleBillboard");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        if (material.HasProperty("_CoreStrength"))
            material.SetFloat("_CoreStrength", 1.45f);
        if (material.HasProperty("_EdgeSoftness"))
            material.SetFloat("_EdgeSoftness", 0.5f);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }
}
