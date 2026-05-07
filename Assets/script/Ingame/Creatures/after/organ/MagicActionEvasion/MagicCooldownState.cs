using UnityEngine;

public class MagicCooldownState : MonoBehaviour
{
    public float cooldown = 3f;
    float lastCastTime = -999f;

    public bool CanCast(float currentMana, float manaCost)
    {
        return currentMana >= manaCost && Time.time >= lastCastTime + Mathf.Max(0f, cooldown);
    }

    public void MarkCast()
    {
        lastCastTime = Time.time;
    }
}
