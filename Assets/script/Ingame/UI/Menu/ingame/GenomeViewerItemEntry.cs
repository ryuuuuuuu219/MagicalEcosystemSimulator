using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GenomePhaseBucket
{
    public List<string> dnaCodes = new List<string>();
}

public class GenomeViewerItemEntry : MonoBehaviour
{
    public TextMeshProUGUI idText;
    public TextMeshProUGUI dnaText;
    public Button applyButton;

    public SpeciesType species;
    public int phaseId;
    public int numberId;
    public byte[] dnaBytes;
    public string dnaCode;

    herbivoreManager herbivoreMgr;
    predatorManager predatorMgr;
    AdvanceGenerationController controller;

    public void Initialize(
        AdvanceGenerationController owner,
        herbivoreManager herbivoreManager,
        predatorManager predatorManager,
        SpeciesType speciesType,
        int phase,
        int number,
        string encodedDna,
        bool isDead)
    {
        controller = owner;
        herbivoreMgr = herbivoreManager;
        predatorMgr = predatorManager;
        species = speciesType;
        phaseId = Mathf.Max(0, phase);
        numberId = Mathf.Max(0, number);
        dnaCode = encodedDna ?? string.Empty;
        dnaBytes = DecodeDnaBytes(dnaCode);

        if (idText != null)
            idText.text = $"{species} P:{phaseId} N:{numberId}";
        if (dnaText != null)
            dnaText.text = dnaCode;
        if (isDead)
        {
            if (idText != null) idText.color = Color.red;
            if (dnaText != null) dnaText.color = Color.red;
        }
        else
        {
            if (idText != null) idText.color = Color.white;
            if (dnaText != null) dnaText.color = Color.white;
        }

        if (applyButton != null)
        {
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(ApplyToManagerGenomes);
        }
    }

    void ApplyToManagerGenomes()
    {
        bool ok = false;
        switch (species)
        {
            case SpeciesType.Herbivore:
                ok = WriteToBuckets(herbivoreMgr != null ? herbivoreMgr.genomes : null, phaseId, numberId, dnaCode);
                break;
            case SpeciesType.Predator:
                ok = WriteToBuckets(predatorMgr != null ? predatorMgr.genomes : null, phaseId, numberId, dnaCode);
                break;
        }

        if (controller != null)
        {
            controller.NotifyGenomeUiStatus(ok
                ? $"Saved {species} DNA to genomes[{phaseId}][{numberId}]"
                : $"Failed to save {species} DNA.");
        }

        if (ok && applyButton != null)
        {
            TextMeshProUGUI label = applyButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
                label.text = "applyed";
        }
    }

    static bool WriteToBuckets(List<GenomePhaseBucket> buckets, int phase, int number, string code)
    {
        if (buckets == null)
            return false;

        while (buckets.Count <= phase)
            buckets.Add(new GenomePhaseBucket());

        var bucket = buckets[phase];
        if (bucket == null)
        {
            bucket = new GenomePhaseBucket();
            buckets[phase] = bucket;
        }

        while (bucket.dnaCodes.Count <= number)
            bucket.dnaCodes.Add(string.Empty);

        bucket.dnaCodes[number] = code ?? string.Empty;
        return true;
    }

    static byte[] DecodeDnaBytes(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Array.Empty<byte>();

        string raw = code.Trim();
        if (raw.StartsWith(GenomeSerializer.HerbivorePrefix, StringComparison.Ordinal))
            raw = raw.Substring(GenomeSerializer.HerbivorePrefix.Length);

        try
        {
            return Convert.FromBase64String(raw);
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }
}

