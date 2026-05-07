using UnityEngine;

public class PreyMemory : MonoBehaviour
{
    public GameObject rememberedPrey;
    public Vector3 rememberedPosition;
    public bool hasMemory;

    public void Remember(GameObject prey)
    {
        if (prey == null) return;
        rememberedPrey = prey;
        rememberedPosition = prey.transform.position;
        hasMemory = true;
    }

    public void Clear()
    {
        rememberedPrey = null;
        rememberedPosition = Vector3.zero;
        hasMemory = false;
    }
}
