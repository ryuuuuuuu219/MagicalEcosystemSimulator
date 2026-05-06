public enum category
{
    grass,
    herbivore,
    predator,
    highpredator,
    dominant
}

public enum AiAgentMode
{
    grassland,
    herbibore,
    predator,
    highpredator,
    dominant
}

public enum EvaluationAxis
{
    Random,
    Mana,
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
