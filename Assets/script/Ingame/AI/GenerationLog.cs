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
}

[Serializable]
public class GenerationLogCollection
{
    public List<GenerationLog> logs = new List<GenerationLog>();
}
