using UnityEngine;

public class FoodMemory : MonoBehaviour
{
    public GameObject rememberedFood;
    public Vector3 rememberedPosition;
    public bool hasMemory;

    public void Remember(GameObject food)
    {
        if (food == null) return;
        rememberedFood = food;
        rememberedPosition = food.transform.position;
        hasMemory = true;
    }

    public void Clear()
    {
        rememberedFood = null;
        rememberedPosition = Vector3.zero;
        hasMemory = false;
    }
}
