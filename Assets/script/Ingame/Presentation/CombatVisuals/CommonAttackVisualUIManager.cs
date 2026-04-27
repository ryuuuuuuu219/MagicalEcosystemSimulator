using UnityEngine;

public class CommonAttackVisualUIManager : WorldUIManager
{
    public static CommonAttackVisualUIManager Instance { get; private set; }

    [Header("Common Attack Trace")]
    [Min(0.5f)] public float attackTraceWidthPixels = 3f;

    [Header("Damage Numbers")]
    [Min(1f)] public float damageNumberFontSize = 28f;
    [Min(0.05f)] public float damageNumberDuration = 0.85f;
    [Min(0f)] public float damageNumberRisePixels = 48f;
    public Vector2 damageNumberScreenOffset = new Vector2(0f, 24f);
    public Color damageNumberColor = new Color(1f, 0.2f, 0.08f, 1f);

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
