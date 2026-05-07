using UnityEngine;

public class CreatureMotorBootstrap : MonoBehaviour
{
    public void Prepare()
    {
        AnimalAICommon.PrepareLegacyRigidbody(gameObject);
    }

    void Awake()
    {
        Prepare();
    }
}
