using System.Text;
using UnityEngine;

public class PerformanceBudgetMonitor : MonoBehaviour
{
    [Header("Measurement")]
    [SerializeField] float warmupSeconds = 3f;
    [SerializeField] float reportIntervalSeconds = 5f;
    [SerializeField] bool autoDisableVSync = true;
    [SerializeField] bool showOverlay = true;
    [SerializeField] bool logErrorOnDrop = true;
    [SerializeField] float watchTargetFps = 60f;

    [Header("Targets")]
    [SerializeField] float targetFpsA = 24f;
    [SerializeField] float targetFpsB = 30f;
    [SerializeField] float targetFpsC = 60f;

    float elapsed;
    float reportElapsed;
    int frameCount;
    float frameTimeSumMs;
    float frameTimeMaxMs;
    string latestSummary = string.Empty;
    bool wasBelowWatchTarget;

    void Start()
    {
        if (autoDisableVSync)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
    }

    void Update()
    {
        float frameMs = Time.unscaledDeltaTime * 1000f;
        DetectDrop(frameMs);

        elapsed += Time.unscaledDeltaTime;

        if (elapsed < warmupSeconds)
            return;

        frameCount++;
        reportElapsed += Time.unscaledDeltaTime;
        frameTimeSumMs += frameMs;
        frameTimeMaxMs = Mathf.Max(frameTimeMaxMs, frameMs);

        if (reportElapsed >= reportIntervalSeconds)
            FlushReport();
    }

    void DetectDrop(float frameMs)
    {
        if (!logErrorOnDrop || watchTargetFps <= 0f)
            return;

        float currentFps = frameMs > 0.0001f ? 1000f / frameMs : 0f;
        bool below = currentFps < watchTargetFps;
        if (below && !wasBelowWatchTarget)
            LogDropCause(currentFps, frameMs);

        wasBelowWatchTarget = below;
    }

    void OnDisable()
    {
        if (frameCount > 0)
            FlushReport();
    }

    void OnGUI()
    {
        if (!showOverlay || string.IsNullOrEmpty(latestSummary))
            return;

        const float width = 640f;
        const float height = 110f;
        GUI.Box(new Rect(12f, 12f, width, height), "Performance Budget Monitor");
        GUI.Label(new Rect(24f, 38f, width - 24f, height - 26f), latestSummary);
    }

    void FlushReport()
    {
        if (frameCount <= 0)
            return;

        float avgFrameMs = frameTimeSumMs / frameCount;
        float avgFps = avgFrameMs > 0.0001f ? 1000f / avgFrameMs : 0f;

        var sb = new StringBuilder();
        sb.Append($"AvgFPS={avgFps:F1} AvgFrame={avgFrameMs:F2}ms MaxFrame={frameTimeMaxMs:F2}ms");
        AppendBudget(sb, targetFpsA, avgFrameMs, frameTimeMaxMs);
        AppendBudget(sb, targetFpsB, avgFrameMs, frameTimeMaxMs);
        AppendBudget(sb, targetFpsC, avgFrameMs, frameTimeMaxMs);

        latestSummary = sb.ToString();
        Debug.Log($"[PerfBudget] {latestSummary}");

        reportElapsed = 0f;
        frameCount = 0;
        frameTimeSumMs = 0f;
        frameTimeMaxMs = 0f;
    }

    static void AppendBudget(StringBuilder sb, float targetFps, float avgFrameMs, float maxFrameMs)
    {
        if (targetFps <= 0f)
            return;

        float budgetMs = 1000f / targetFps;
        float usageAvg = avgFrameMs / budgetMs * 100f;
        float usageMax = maxFrameMs / budgetMs * 100f;
        bool passAvg = usageAvg <= 100f;

        sb.Append(
            $" | {targetFps:F0}fps: AvgLoad={usageAvg:F1}% PeakLoad={usageMax:F1}% {(passAvg ? "OK" : "OVER")}"
        );
    }

    void LogDropCause(float currentFps, float frameMs)
    {
        GameObject culprit = FindLikelyCulprit(out string reason, out float score);
        string message =
            $"[PerfBudgetDrop] FPS dropped below target. fps={currentFps:F1} frame={frameMs:F2}ms target={watchTargetFps:F0} " +
            $"culpritScore={score:F1} reason={reason}";

        if (culprit != null)
            Debug.LogWarning(message, culprit);
        else
            Debug.LogWarning(message);
    }

    GameObject FindLikelyCulprit(out string reason, out float score)
    {
        reason = "no candidate";
        score = 0f;

        herbivoreManager hm = FindFirstObjectByType<herbivoreManager>();
        predatorManager pm = FindFirstObjectByType<predatorManager>();
        ResourceDispenser rd = FindFirstObjectByType<ResourceDispenser>();

        int grassCount = CountAlive(rd != null ? rd.grasses : null);
        int herbivoreCount = CountAlive(hm != null ? hm.herbivores : null);
        int predatorCount = CountAlive(pm != null ? pm.predators : null);

        GameObject bestObj = null;
        string bestReason = "no candidate";
        float bestScore = 0f;

        if (hm != null && hm.herbivores != null)
        {
            for (int i = 0; i < hm.herbivores.Count; i++)
            {
                GameObject obj = hm.herbivores[i];
                if (obj == null || !obj.TryGetComponent<herbivoreBehaviour>(out var hb))
                    continue;

                int nearbyPredators = CountNearby(pm != null ? pm.predators : null, obj.transform.position, hb.genome.threatDetectDistance);
                float est = grassCount * Mathf.Max(1, nearbyPredators);
                est += nearbyPredators * predatorCount;

                if (est > bestScore)
                {
                    bestScore = est;
                    bestObj = obj;
                    bestReason = $"herbivore vision-safecheck pressure grass={grassCount} nearPred={nearbyPredators} pred={predatorCount}";
                }
            }
        }

        if (pm != null && pm.predators != null)
        {
            for (int i = 0; i < pm.predators.Count; i++)
            {
                GameObject obj = pm.predators[i];
                if (obj == null || !obj.TryGetComponent<predatorBehaviour>(out var pb))
                    continue;

                int nearbyPrey = CountNearby(hm != null ? hm.herbivores : null, obj.transform.position, pb.genome.preyDetectDistance);
                int nearbyThreat = CountNearby(pm.predators, obj.transform.position, pb.genome.threatDetectDistance, obj);
                float est = nearbyPrey * herbivoreCount + nearbyThreat * predatorCount;
                est += nearbyPrey * Mathf.Max(1, nearbyThreat);

                if (est > bestScore)
                {
                    bestScore = est;
                    bestObj = obj;
                    bestReason = $"predator detection pressure nearPrey={nearbyPrey} nearThreat={nearbyThreat} herb={herbivoreCount} pred={predatorCount}";
                }
            }
        }

        reason = bestReason;
        score = bestScore;
        return bestObj;
    }

    static int CountAlive(System.Collections.Generic.List<GameObject> list)
    {
        if (list == null)
            return 0;

        int count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                count++;
        }

        return count;
    }

    static int CountNearby(
        System.Collections.Generic.List<GameObject> list,
        Vector3 center,
        float radius,
        GameObject exclude = null)
    {
        if (list == null || radius <= 0f)
            return 0;

        float sqr = radius * radius;
        int count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            GameObject obj = list[i];
            if (obj == null || obj == exclude)
                continue;

            Vector3 d = obj.transform.position - center;
            d.y = 0f;
            if (d.sqrMagnitude <= sqr)
                count++;
        }

        return count;
    }
}
