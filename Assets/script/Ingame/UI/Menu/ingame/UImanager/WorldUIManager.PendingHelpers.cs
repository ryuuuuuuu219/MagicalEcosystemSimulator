using System.Reflection;

public partial class WorldUIManager
{
    bool GetPerformanceOverlayVisible(PerformanceBudgetMonitor source)
    {
        return GetMonitorField(source, "showOverlay", true);
    }

    bool GetPerformanceDropLoggingEnabled(PerformanceBudgetMonitor source)
    {
        return GetMonitorField(source, "logErrorOnDrop", true);
    }

    float GetPerformanceWatchTarget(PerformanceBudgetMonitor source)
    {
        return GetMonitorField(source, "watchTargetFps", 60f);
    }

    void SetPerformanceMonitorVisibilityInternal(PerformanceBudgetMonitor source, bool overlayVisible)
    {
        SetMonitorField(source, "showOverlay", overlayVisible);
    }

    void SetPerformanceDropLoggingInternal(PerformanceBudgetMonitor source, bool enabled, float targetFps)
    {
        SetMonitorField(source, "logErrorOnDrop", enabled);
        SetMonitorField(source, "watchTargetFps", targetFps < 1f ? 1f : targetFps);
    }

    static TValue GetMonitorField<TValue>(PerformanceBudgetMonitor source, string fieldName, TValue fallback)
    {
        if (source == null || string.IsNullOrEmpty(fieldName))
            return fallback;

        FieldInfo field = typeof(PerformanceBudgetMonitor).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null || field.FieldType != typeof(TValue))
            return fallback;

        object value = field.GetValue(source);
        return value is TValue typed ? typed : fallback;
    }

    static void SetMonitorField<TValue>(PerformanceBudgetMonitor source, string fieldName, TValue value)
    {
        if (source == null || string.IsNullOrEmpty(fieldName))
            return;

        FieldInfo field = typeof(PerformanceBudgetMonitor).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            return;

        field.SetValue(source, value);
    }
}
