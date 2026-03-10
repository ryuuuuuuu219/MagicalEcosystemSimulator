using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class WorldUIManager
{
    public void ClearStateview()
    {
        viewDisplayFoundation.SetActive(false);
        SetChildrenActive(viewDisplayFoundation, false);
        UI_freq.text = string.Empty;
        currentHerbivoreDnaCode = string.Empty;
        DestroyHerbivoreDnaCopyButton();
        ClearTextureTransparent();
    }

    public void Onclick_State()
    {
        isStatusVisible = !isStatusVisible;
        if (IsStateViewVisible)
        {

            foreach(var item in StatusinfoUIlist)
            {
                item.SetActive(true);
            }

            Stateview(stateViewPage);
        }
        else
        {
            foreach (var item in StatusinfoUIlist)
            {
                item.SetActive(false);
            }
        }
    }

    public void Onclick_PageUp()
    {
        if (!IsStateViewVisible) return;

        stateViewPage = (stateViewPage + 1) % stateViewPageCount;
        Stateview(stateViewPage);
    }

    public void Onclick_PageDown()
    {
        if (!IsStateViewVisible) return;

        stateViewPage = (stateViewPage - 1 + stateViewPageCount) % stateViewPageCount;
        Stateview(stateViewPage);
    }

    void HideStatusUI()
    {
        foreach (var go in StatusUIlist)
        {
            if (go == null) continue;
            go.SetActive(false);
        }
    }

    void SetChildrenActive(GameObject parent, bool active)
    {
        if (parent == null) return;

        Transform t = parent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            t.GetChild(i).gameObject.SetActive(active);
        }
    }

    void InitWaveTexture(int width, int height)
    {
        waveTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveTexture.filterMode = FilterMode.Point;

        pixelBuffer = new Color32[width * height];

        waveImage.texture = waveTexture;
    }

    public void Stateview(int page)
    {
        if (currentTarget == null)
        {
            ClearStateview();
            return;
        }

        WaveGene[] waveGenes = null;
        string pageTitle = string.Empty;
        string detail = string.Empty;

        if (currentTarget.TryGetComponent<herbivoreBehaviour>(out herbivoreBehaviour hb))
        {
            switch (page)
            {
                case 0:
                    waveGenes = hb.genome.visionWaves;
                    pageTitle = "visionWaves";
                    currentHerbivoreDnaCode = string.Empty;
                    break;
                case 1:
                    waveGenes = hb.genome.wanderWaves;
                    pageTitle = "wanderWaves";
                    currentHerbivoreDnaCode = string.Empty;
                    break;
                case 4:
                    pageTitle = "DNA Code";
                    currentHerbivoreDnaCode = GenomeSerializer.EncodeGenome(hb.genome);
                    detail = currentHerbivoreDnaCode;
                    EnsureHerbivoreDnaCopyButton();
                    break;
                default:
                    pageTitle = "status";
                    currentHerbivoreDnaCode = string.Empty;
                    detail =
                        "forwardForce:" + hb.genome.forwardForce.ToString("F2") +
                        "\nturnForce:" + hb.genome.turnForce.ToString("F2") +
                        "\nvisionDistance:" + hb.genome.visionDistance.ToString("F2") +
                        "\nthreatWeight:" + hb.genome.threatWeight.ToString("F2") +
                        "\nrunAwayDistance:" + hb.genome.runAwayDistance.ToString("F2");
                    break;
            }
        }
        else if (currentTarget.TryGetComponent<predatorBehaviour>(out predatorBehaviour pb))
        {
            switch (page)
            {
                case 0:
                    waveGenes = pb.genome.visionWaves;
                    pageTitle = "visionWaves";
                    currentHerbivoreDnaCode = string.Empty;
                    break;
                case 1:
                    waveGenes = pb.genome.wanderWaves;
                    pageTitle = "wanderWaves";
                    currentHerbivoreDnaCode = string.Empty;
                    break;
                case 4:
                    pageTitle = "DNA Code";
                    currentHerbivoreDnaCode = string.Empty;
                    detail = "Predator DNA export is not supported.";
                    break;
                default:
                    pageTitle = "status";
                    currentHerbivoreDnaCode = string.Empty;
                    detail =
                        "forwardForce:" + pb.genome.forwardForce.ToString("F2") +
                        "\nturnForce:" + pb.genome.turnForce.ToString("F2") +
                        "\nvisionDistance:" + pb.genome.visionDistance.ToString("F2") +
                        "\nchaseWeight:" + pb.genome.chaseWeight.ToString("F2") +
                        "\nattackDamage:" + pb.genome.attackDamage.ToString("F2");
                    break;
            }
        }

        if (waveGenes != null)
        {
            DestroyHerbivoreDnaCopyButton();
            DrawWave(waveGenes, page);
            text_f.text =
                "page:" + (page + 1).ToString() + "/" + stateViewPageCount.ToString() +
                "\n" + pageTitle;
            UI_freq.text = "\nwaveCount:" + waveGenes.Length.ToString();
        }
        else
        {
            if (!(currentTarget.TryGetComponent<herbivoreBehaviour>(out _) && page == 4))
                DestroyHerbivoreDnaCopyButton();
            ClearTextureTransparent();
            text_f.text =
                "page:" + (page + 1).ToString() + "/" + stateViewPageCount.ToString() +
                "\n" + pageTitle;
            UI_freq.text = detail;
        }
    }

    public void Onclick_CopyDNA()
    {
        if (string.IsNullOrEmpty(currentHerbivoreDnaCode)) return;
        GUIUtility.systemCopyBuffer = currentHerbivoreDnaCode;
    }

    void EnsureHerbivoreDnaCopyButton()
    {
        if (herbivoreDnaCopyButton != null)
            return;

        GameObject defaultButtonObj = buttonPrefab != null ? buttonPrefab : GameObject.Find("Button");
        if (defaultButtonObj == null)
            return;

        Button defaultButton = defaultButtonObj.GetComponent<Button>();
        if (defaultButton == null)
            defaultButton = defaultButtonObj.GetComponentInChildren<Button>(true);
        if (defaultButton == null)
            return;

        GameObject instance = Object.Instantiate(defaultButton.gameObject, viewDisplayFoundation.transform);
        instance.name = "HerbivoreDnaCopyButton";
        herbivoreDnaCopyButton = instance.GetComponent<Button>();
        if (herbivoreDnaCopyButton == null)
            herbivoreDnaCopyButton = instance.GetComponentInChildren<Button>(true);
        if (herbivoreDnaCopyButton == null)
        {
            Object.Destroy(instance);
            return;
        }

        herbivoreDnaCopyButton.onClick.RemoveAllListeners();
        herbivoreDnaCopyButton.onClick.AddListener(Onclick_CopyDNA);

        TextMeshProUGUI label = herbivoreDnaCopyButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null) label.text = "Copy";

        RectTransform rt = herbivoreDnaCopyButton.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 12f);
        }
    }

    void DestroyHerbivoreDnaCopyButton()
    {
        if (herbivoreDnaCopyButton == null)
            return;

        Object.Destroy(herbivoreDnaCopyButton.gameObject);
        herbivoreDnaCopyButton = null;
    }

    void ClearTextureTransparent()
    {
        if (waveTexture == null || pixelBuffer == null)
            return;

        System.Array.Fill(pixelBuffer, new Color32(0, 0, 0, 0));

        waveTexture.SetPixels32(pixelBuffer);
        waveTexture.Apply();
    }

    void DrawWave(WaveGene[] waveGenes, int page)
    {
        if (waveGenes == null || waveGenes.Length == 0)
        {
            ClearTextureTransparent();
            return;
        }

        int width = waveTexture.width;
        int height = waveTexture.height;

        for (int i = 0; i < pixelBuffer.Length; i++)
            pixelBuffer[i] = new Color32(0, 0, 0, 0);

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        float[] samples = new float[width];

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1) * Mathf.PI * 2f;
            float y = returny(waveGenes, t);

            samples[x] = y;

            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        float range = maxY - minY;
        if (range < 0.0001f) range = 1f;

        for (int x = 0; x < width; x++)
        {
            float normalized = (samples[x] - minY) / range;
            int yPixel = Mathf.RoundToInt(normalized * (height - 1));

            int index = yPixel * width + x;

            if (index >= 0 && index < pixelBuffer.Length)
                pixelBuffer[index] = new Color32(255, 255, 255, 255);
        }

        waveTexture.SetPixels32(pixelBuffer);
        waveTexture.Apply();
    }

    float returny(WaveGene[] waveGenes, float x)
    {
        float value = 0f;

        foreach (var w in waveGenes)
        {
            value += w.amplitude * Mathf.Sin(w.frequency * x + w.phase);
        }

        return value;
    }
}
