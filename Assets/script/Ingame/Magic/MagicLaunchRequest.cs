using UnityEngine;

public struct MagicLaunchRequest
{
    public MagicElement element;
    public Vector3 origin;
    public Vector3 direction;
    public float spawnOffset;
    public float projectileScale;
    public MagicProjectileLaunchSettings projectileSettings;
    public MagicCircleLaunchSettings spellCircleSettings;
    public string projectileName;
    public Resource casterResource;

    public Vector3 Direction => direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
    public Vector3 SpawnPosition => origin + Direction * spawnOffset;
}
