using System.Collections.Generic;
using UnityEngine;

public class AIMemoryStore : MonoBehaviour
{
    [System.Serializable]
    public struct MemoryEntry
    {
        public GameObject obj;
        public Vector3 position;
        public float time;
    }

    public List<MemoryEntry> entries = new();

    public void Remember(GameObject obj, float memoryTime)
    {
        if (obj == null) return;

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].obj == obj)
            {
                entries[i] = new MemoryEntry { obj = obj, position = obj.transform.position, time = memoryTime };
                return;
            }
        }

        entries.Add(new MemoryEntry { obj = obj, position = obj.transform.position, time = memoryTime });
    }

    public void Tick(float deltaTime)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            MemoryEntry entry = entries[i];
            entry.time -= deltaTime;
            if (entry.time <= 0f || entry.obj == null)
                entries.RemoveAt(i);
            else
                entries[i] = entry;
        }
    }
}
