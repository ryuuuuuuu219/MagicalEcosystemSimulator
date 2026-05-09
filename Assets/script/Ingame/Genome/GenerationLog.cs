using System;
using System.Collections.Generic;

[Serializable]
public class GenerationLog
{
    public int generation;
    public string timestamp;
    public int population;
    public string bestGenome;
    public float bestFitness;
    public string bestOrganCheckpointReason;
    public int bestOrganGeneCount;
    public int bestActiveOrganGeneCount;
    public int bestVestigialOrganCount;
    public float bestOrganCheckpointScore;
    public string bestActiveOrgans;
    public string bestVestigialOrgans;
    public string generationOrganMutations;
}

[Serializable]
public class GenerationLogCollection
{
    public List<GenerationLog> logs = new List<GenerationLog>();
}
