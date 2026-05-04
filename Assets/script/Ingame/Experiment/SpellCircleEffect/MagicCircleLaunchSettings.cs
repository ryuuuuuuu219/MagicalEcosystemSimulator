using UnityEngine;

[System.Serializable]
public struct MagicCircleLaunchSettings
{
    public bool enabled;
    public float chargeDuration;
    public float visibleLifetime;
    public float radius;
    public float forwardOffset;
    public float lineWidth;
    public float minFractalScale;
    public int maxFractalIterations;
    public Color lineColor;

    public static MagicCircleLaunchSettings Default => new MagicCircleLaunchSettings
    {
        enabled = true,
        chargeDuration = 0.45f,
        visibleLifetime = 0.75f,
        radius = 1.35f,
        forwardOffset = 0.12f,
        lineWidth = 0.018f,
        minFractalScale = 0.16f,
        maxFractalIterations = 7,
        lineColor = new Color(1f, 1f, 1f, 0.5f)
    };
}
