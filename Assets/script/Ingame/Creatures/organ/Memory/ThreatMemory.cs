using UnityEngine;

public class ThreatMemory : MonoBehaviour
{
    public GameObject rememberedThreat;
    public Vector3 rememberedPosition;
    public bool hasMemory;

    public void Remember(GameObject threat)
    {
        if (threat == null) return;
        rememberedThreat = threat;
        rememberedPosition = threat.transform.position;
        hasMemory = true;
    }

    public void Clear()
    {
        rememberedThreat = null;
        rememberedPosition = Vector3.zero;
        hasMemory = false;
    }
}
