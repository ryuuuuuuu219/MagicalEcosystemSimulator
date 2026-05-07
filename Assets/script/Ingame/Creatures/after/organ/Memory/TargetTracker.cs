using UnityEngine;

public class TargetTracker : MonoBehaviour
{
    public GameObject target;
    public Vector3 lastPosition;
    public Vector3 velocity;
    public bool hasSample;

    public void Track(GameObject nextTarget, float deltaTime)
    {
        if (nextTarget == null)
        {
            target = null;
            hasSample = false;
            velocity = Vector3.zero;
            return;
        }

        if (target != nextTarget)
        {
            target = nextTarget;
            lastPosition = nextTarget.transform.position;
            velocity = Vector3.zero;
            hasSample = false;
            return;
        }

        float dt = Mathf.Max(0.0001f, deltaTime);
        Vector3 current = nextTarget.transform.position;
        velocity = (current - lastPosition) / dt;
        velocity.y = 0f;
        lastPosition = current;
        hasSample = true;
    }
}
