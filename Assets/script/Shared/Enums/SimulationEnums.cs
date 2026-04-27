public enum category
{
    grass,
    herbivore,
    predator
}

public enum AiAgentMode
{
    grassland,
    herbibore,
    predator,
    highpredator
}

public enum EvaluationAxis
{
    Random,
    Carbon,
    Health,
    Selection
}

public enum CrossoverMode
{
    Assign,
    Average,
    Interpolate,
    Mix
}

public enum MutationRangeMode
{
    GlobalRange,
    ParentRelative
}

public enum GenomeInputMode
{
    Population,
    SavedGenome,
    ManagerGenome
}

public enum GenerationPhase
{
    Both,
    HerbivoreOnly,
    PredatorOnly
}

public enum SpeciesType
{
    Herbivore,
    Predator
}
