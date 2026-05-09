[System.Serializable]
public struct AIComponentGene
{
    public string componentId;
    public bool enabled;
    public bool isVitalOrgan;
    public bool isVestigialOrgan;
    public float level;
    public float weight;
    public float installChance;
    public float mutationChanceT;
    public float mutationChanceG;
    public float minLevel;
    public float maxLevel;

    public bool IsActive => enabled && !isVestigialOrgan && level > 0f && weight > 0f;

    public static AIComponentGene CreateDefault(string componentId, bool enabled = true, bool isVitalOrgan = false)
    {
        return new AIComponentGene
        {
            componentId = componentId,
            enabled = enabled,
            isVitalOrgan = isVitalOrgan,
            isVestigialOrgan = false,
            level = 1f,
            weight = 1f,
            installChance = 1f,
            mutationChanceT = 0f,
            mutationChanceG = 0f,
            minLevel = 0f,
            maxLevel = 4f
        };
    }
}
