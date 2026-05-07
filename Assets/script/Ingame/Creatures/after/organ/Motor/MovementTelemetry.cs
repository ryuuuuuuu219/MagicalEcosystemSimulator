[System.Serializable]
public struct MovementTelemetry
{
    public float moveDemand;
    public float accelerationDemand;
    public float brakingDemand;
    public float turnDemand;

    public static MovementTelemetry From(AnimalAICommon.MovementTelemetry source)
    {
        return new MovementTelemetry
        {
            moveDemand = source.moveDemand,
            accelerationDemand = source.accelerationDemand,
            brakingDemand = source.brakingDemand,
            turnDemand = source.turnDemand
        };
    }
}
