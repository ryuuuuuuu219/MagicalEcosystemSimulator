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
        CreateParticleImpact(point, normal);

        if (wrapNonTerrainTargets && target.GetComponent<TerrainCollider>() == null)
        {
            MagicImpactEnvelopeEffect.Create(target, impactMaterialColor, envelopeLifetime, envelopePadding);
            Debug.Log($"{element} projectile wrapped target: target={target.name}, point={point}, radius={effectRadius}");
            Destroy(gameObject);
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

        Destroy(gameObject);
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
