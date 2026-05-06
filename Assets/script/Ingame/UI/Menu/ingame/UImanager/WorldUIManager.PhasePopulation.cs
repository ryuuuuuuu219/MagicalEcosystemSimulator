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

        UpdatePhasePopulationText(true);
    }

    void UpdatePhasePopulationText(bool force = false)
    {
        if (phasePopulationText == null)
            return;

        if (!force && Time.time < nextPhasePopulationUpdateTime)
            return;

        nextPhasePopulationUpdateTime = Time.time + phasePopulationUpdateInterval;

        int herbivoreCount = 0;
        int predatorCount = 0;
        int highPredatorCount = 0;
        int dominantCount = 0;

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

        phasePopulationText.text =
            "phase counts\n" +
            $"herbivore: {herbivoreCount}\n" +
            $"predator: {predatorCount}\n" +
            $"high: {highPredatorCount}\n" +
            $"dominant: {dominantCount}";
    }
}
