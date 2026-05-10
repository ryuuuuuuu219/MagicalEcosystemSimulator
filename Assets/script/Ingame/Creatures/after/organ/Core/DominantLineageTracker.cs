using System.Collections.Generic;

public static class DominantLineageTracker
{
    static readonly Dictionary<int, int> naturalDominantCounts = new();

    public static int TotalNaturalDominantCount
    {
        get
        {
            int total = 0;
            foreach (int count in naturalDominantCounts.Values)
                total += count;
            return total;
        }
    }

    public static void RecordNaturalDominant(int speciesID)
    {
        if (!naturalDominantCounts.ContainsKey(speciesID))
            naturalDominantCounts[speciesID] = 0;
        naturalDominantCounts[speciesID]++;
    }

    public static int ClampDominantSpawnCount(int requestedCount)
    {
        if (requestedCount <= 0)
            return 0;
        return System.Math.Min(requestedCount, TotalNaturalDominantCount);
    }
}
