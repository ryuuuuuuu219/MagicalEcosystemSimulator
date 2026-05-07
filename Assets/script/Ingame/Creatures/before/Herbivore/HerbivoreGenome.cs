[System.Serializable]

public struct HerbivoreGenome

{

    public float forwardForce;

    public float turnForce;

    public float visionAngle;

    public float visionturnAngle;

    public float visionDistance;

    public float metabolismRate;

    public float eatspeed;

    public float threatWeight;

    public float threatDetectDistance;

    public float memorytime;

    public float runAwayDistance;

    public float contactEscapeDistance;

    public float evasionAngle;

    public float evasionDuration;

    public float evasionCooldown;

    public float evasionDistance;

    public bool predictIntercept;

    public float zigzagFrequency;

    public float zigzagAmplitude;

    public float foodWeight;

    public float predatorWeight;

    public float corpseWeight;

    public float fearThreshold;

    public float escapeThreshold;

    public float curiosity;



    public WaveGene[] visionWaves;

    public WaveGene[] wanderWaves;

}

