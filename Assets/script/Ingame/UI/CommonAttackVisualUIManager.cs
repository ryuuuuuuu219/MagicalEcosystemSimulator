using UnityEngine;

public class CommonAttackVisualUIManager : WorldUIManager
{
    public static CommonAttackVisualUIManager Instance { get; private set; }

    [Header("Common Attack Trace")]
    [Min(0.5f)] public float attackTraceWidthPixels = 3f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
