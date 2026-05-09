using TMPro;
using UnityEngine;

public partial class WorldUIManager
{
    const float phasePopulationUpdateInterval = 0.5f;

    void InitializePhasePopulationText()
    {
        if (phasePopulationText == null && disturbanceTab != null)
        {
            Transform textRoot = disturbanceTab.transform.Find("PhasePopulationText");
            if (textRoot != null)
                phasePopulationText = textRoot.GetComponent<TextMeshProUGUI>();
        }

        KeepPhasePopulationTextVisible();
        UpdatePhasePopulationText(true);
    }

    void KeepPhasePopulationTextVisible()
    {
        if (phasePopulationText == null)
            return;

        Transform textTransform = phasePopulationText.transform;
        if (disturbanceTab != null && textTransform.IsChildOf(disturbanceTab.transform))
        {
            Transform alwaysVisibleParent = disturbanceTab.transform.parent;
            if (alwaysVisibleParent != null)
            {
                textTransform.SetParent(alwaysVisibleParent, true);
                textTransform.SetAsLastSibling();
            }
        }

        phasePopulationText.gameObject.SetActive(true);
    }

    void UpdatePhasePopulationText(bool force = false)
    {
        if (phasePopulationText == null)
            return;

        if (!force && Time.time < nextPhasePopulationUpdateTime)
            return;

        nextPhasePopulationUpdateTime = Time.time + phasePopulationUpdateInterval;

        GetPhasePopulationCounts(
            out int herbivoreCount,
            out int predatorCount,
            out int highPredatorCount,
            out int dominantCount);

        phasePopulationText.text =
            "phase counts\n" +
            $"herbivore: {herbivoreCount}\n" +
            $"predator: {predatorCount}\n" +
            $"high: {highPredatorCount}\n" +
            $"dominant: {dominantCount}";
    }

    string BuildPhasePopulationSummary()
    {
        GetPhasePopulationCounts(
            out int herbivoreCount,
            out int predatorCount,
            out int highPredatorCount,
            out int dominantCount);

        return $"Current Phase Counts: H={herbivoreCount} P={predatorCount} High={highPredatorCount} Dominant={dominantCount}";
    }

    void GetPhasePopulationCounts(
        out int herbivoreCount,
        out int predatorCount,
        out int highPredatorCount,
        out int dominantCount)
    {
        herbivoreCount = 0;
        predatorCount = 0;
        highPredatorCount = 0;
        dominantCount = 0;

        herbivoreBehaviour[] herbivores = FindObjectsByType<herbivoreBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < herbivores.Length; i++)
        {
            herbivoreBehaviour herbivore = herbivores[i];
            if (herbivore != null && !herbivore.IsDead)
                herbivoreCount++;
        }

        predatorBehaviour[] predators = FindObjectsByType<predatorBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < predators.Length; i++)
        {
            predatorBehaviour predator = predators[i];
            if (predator == null || predator.IsDead)
                continue;

            category phase = category.predator;
            if (predator.TryGetComponent<Resource>(out var resource))
                phase = resource.resourceCategory;

            switch (phase)
            {
                case category.highpredator:
                    highPredatorCount++;
                    break;
                case category.dominant:
                    dominantCount++;
                    break;
                default:
                    predatorCount++;
                    break;
            }
        }
    }
}
