using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Resource;

public class AdvanceGenerationController : MonoBehaviour
{
    public enum EvaluationAxis
    {
        Random,
        Carbon,
        Health,
        Selection
    }

    public enum CrossoverMode
    {
        Assign,
        Average,
        Interpolate,
        Mix
    }

    public enum MutationRangeMode
    {
        GlobalRange,
        ParentRelative
    }

    public enum GenomeInputMode
    {
        Population,
        SavedGenome,
        ManagerGenome
    }

    public enum GenerationPhase
    {
        Both,
        HerbivoreOnly,
        PredatorOnly
    }

    [Header("References")]
    public GameObject worldgen;
    public ResourceDispenser resourceDispenser;
    public herbivoreManager herbivoreManager;
    public predatorManager predatorManager;

    [Header("Selection")]
    public EvaluationAxis evaluationAxis = EvaluationAxis.Carbon;
    public GenomeInputMode herbivoreInputMode = GenomeInputMode.Population;
    public GenomeInputMode predatorInputMode = GenomeInputMode.Population;
    public GenerationPhase generationPhase = GenerationPhase.Both;

    [Header("Crossover")]
    public bool enableCrossover = true;
    public CrossoverMode crossoverMode = CrossoverMode.Average;

    [Header("Mutation")]
    public bool enableMutation = true;
    [Range(0f, 1f)] public float mutationChance = 0.1f;
    public MutationRangeMode mutationRangeMode = MutationRangeMode.ParentRelative;
    public float globalMutationMin = -0.2f;
    public float globalMutationMax = 0.2f;
    public float parentMutationScale = 0.2f;

    [Header("Saved Genomes")]
    public HerbivoreGenome savedHerbivoreGenome;
    public PredatorGenome savedPredatorGenome;
    public bool saveOutputGenome = true;

    [Header("Generation")]
    public int generationIndex = 1;
    int injectedHerbivoreSpawnIndex = 1000000;
    [SerializeField] int selectedPhasePage = 0;
    [SerializeField] int selectedGenerationPage = 0;
    const int generationPageSize = 12;
    GenomeViewerItemEntry.SpeciesType SelectedSpecies => (selectedPhasePage % 2 == 0)
        ? GenomeViewerItemEntry.SpeciesType.Herbivore
        : GenomeViewerItemEntry.SpeciesType.Predator;
    int SelectedPhaseIndex => Mathf.Max(0, selectedPhasePage / 2);

    [Header("Genome UI")]
    public GameObject herbivoreGenomeViewerRoot;
    public Transform herbivoreGenomeViewerContent;
    public GameObject herbivoreGenomeViewerItemPrefab;
    public GameObject herbivoreGenomeInjectorRoot;
    public TMP_InputField herbivoreGenomeInjectorInput;
    public TextMeshProUGUI herbivoreGenomeUiStatusText;
    TextMeshProUGUI herbivoreGenomePageStatusText;

    readonly List<GameObject> createdGenomeUi = new List<GameObject>();

    void Awake()
    {
        EnsureGenomeViewerItemPrefab();
        EnsureGenomeUi();
    }

    void EnsureGenomeUi()
    {
        EnsureGenomeViewerItemPrefab();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        if (herbivoreGenomeViewerRoot == null)
        {
            herbivoreGenomeViewerRoot = CreatePanel("HerbivoreGenomeViewerRoot", canvas.transform, new Vector2(0.02f, 0.08f), new Vector2(0.48f, 0.62f));
            herbivoreGenomeViewerRoot.SetActive(false);
        }

        if (herbivoreGenomeViewerContent == null)
            herbivoreGenomeViewerContent = EnsureViewerContent(herbivoreGenomeViewerRoot.transform);
        EnsureViewerPageControls(herbivoreGenomeViewerRoot.transform);

        if (herbivoreGenomeInjectorRoot == null)
        {
            herbivoreGenomeInjectorRoot = CreatePanel("HerbivoreGenomeInjectorRoot", canvas.transform, new Vector2(0.52f, 0.08f), new Vector2(0.98f, 0.30f));
            herbivoreGenomeInjectorRoot.SetActive(false);
        }

        if (herbivoreGenomeInjectorInput == null || herbivoreGenomeUiStatusText == null)
            EnsureInjectorWidgets(herbivoreGenomeInjectorRoot.transform);
    }

    void EnsureGenomeViewerItemPrefab()
    {
        if (herbivoreGenomeViewerItemPrefab != null &&
            herbivoreGenomeViewerItemPrefab.GetComponent<GenomeViewerItemEntry>() != null)
            return;

        herbivoreGenomeViewerItemPrefab = CreateGenomeViewerItemPrefab();
    }

    GameObject CreateGenomeViewerItemPrefab()
    {
        var item = new GameObject("HerbivoreGenomeViewerItemPrefab", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        item.transform.SetParent(transform, false);
        item.SetActive(false);

        item.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.04f);
        item.GetComponent<LayoutElement>().preferredHeight = 88f;

        var idLabel = CreateLabel("IdLabel", item.transform, string.Empty, new Vector2(0.02f, 0.56f), new Vector2(0.74f, 0.96f));
        idLabel.fontSize = 18;
        idLabel.textWrappingMode = TextWrappingModes.NoWrap;

        var dnaLabel = CreateLabel("DnaLabel", item.transform, string.Empty, new Vector2(0.02f, 0.06f), new Vector2(0.74f, 0.52f));
        dnaLabel.fontSize = 14;

        Button apply = CreateButton(item.transform, "ApplyButton", "Apply", new Vector2(0.76f, 0.2f), new Vector2(0.98f, 0.84f));
        var applyText = apply.GetComponentInChildren<TextMeshProUGUI>(true);
        if (applyText != null)
            applyText.fontSize = 16;

        var entry = item.AddComponent<GenomeViewerItemEntry>();
        entry.idText = idLabel;
        entry.dnaText = dnaLabel;
        entry.applyButton = apply;
        return item;
    }

    GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var bg = go.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.55f);
        return go;
    }

    Transform EnsureViewerContent(Transform root)
    {
        var title = CreateLabel("Title", root, "Herbivore Genome Viewer", new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.98f));
        title.alignment = TextAlignmentOptions.MidlineLeft;
        EnsureViewerPageControls(root);

        var scroll = new GameObject("ScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scroll.transform.SetParent(root, false);
        var scrollRt = scroll.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.243518785f, 0.0933779255f);
        scrollRt.anchorMax = new Vector2(0.952000022f, 0.927689016f);
        scrollRt.offsetMin = Vector2.zero;
        scrollRt.offsetMax = Vector2.zero;
        scroll.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.08f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scroll.transform, false);
        var viewportRt = viewport.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 4f;
        vlg.padding = new RectOffset(6, 6, 6, 6);

        var fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var scrollRect = scroll.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        return content.transform;
    }

    void EnsureInjectorWidgets(Transform root)
    {
        CreateLabel("Title", root, "Spawn Herbivore From DNA", new Vector2(0.03f, 0.72f), new Vector2(0.97f, 0.98f));

        if (herbivoreGenomeInjectorInput == null)
            herbivoreGenomeInjectorInput = CreateInputField(root, "GenomeInput", new Vector2(0.03f, 0.38f), new Vector2(0.97f, 0.68f), "Paste DNA code...");

        var button = CreateButton(root, "SpawnButton", "Spawn", new Vector2(0.03f, 0.16f), new Vector2(0.38f, 0.32f));
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnclickSpawnHerbivoreGenome);

        if (herbivoreGenomeUiStatusText == null)
            herbivoreGenomeUiStatusText = CreateLabel("Status", root, string.Empty, new Vector2(0.42f, 0.03f), new Vector2(0.97f, 0.32f));
    }

    void EnsureViewerPageControls(Transform root)
    {
        if (root.Find("PhasePrevButton") == null)
        {
            Button phasePrev = CreateButton(root, "PhasePrevButton", "Phase -", new Vector2(0.03f, 0.86f), new Vector2(0.16f, 0.98f));
            phasePrev.onClick.RemoveAllListeners();
            phasePrev.onClick.AddListener(OnclickPhasePageDown);
        }

        if (root.Find("PhaseNextButton") == null)
        {
            Button phaseNext = CreateButton(root, "PhaseNextButton", "Phase +", new Vector2(0.17f, 0.86f), new Vector2(0.30f, 0.98f));
            phaseNext.onClick.RemoveAllListeners();
            phaseNext.onClick.AddListener(OnclickPhasePageUp);
        }

        if (root.Find("GenerationPrevButton") == null)
        {
            Button genPrev = CreateButton(root, "GenerationPrevButton", "Gen -", new Vector2(0.31f, 0.86f), new Vector2(0.44f, 0.98f));
            genPrev.onClick.RemoveAllListeners();
            genPrev.onClick.AddListener(OnclickGenerationPageDown);
        }

        if (root.Find("GenerationNextButton") == null)
        {
            Button genNext = CreateButton(root, "GenerationNextButton", "Gen +", new Vector2(0.45f, 0.86f), new Vector2(0.58f, 0.98f));
            genNext.onClick.RemoveAllListeners();
            genNext.onClick.AddListener(OnclickGenerationPageUp);
        }

        if (herbivoreGenomePageStatusText == null)
            herbivoreGenomePageStatusText = CreateLabel("PageStatus", root, string.Empty, new Vector2(0.60f, 0.86f), new Vector2(0.97f, 0.98f));
    }

    public void OnclickPhasePageUp()
    {
        selectedPhasePage = Mathf.Max(0, selectedPhasePage + 1);
        if (herbivoreGenomeViewerRoot != null && herbivoreGenomeViewerRoot.activeSelf)
            BuildHerbivoreGenomeViewerList();
    }

    public void OnclickPhasePageDown()
    {
        selectedPhasePage = Mathf.Max(0, selectedPhasePage - 1);
        if (herbivoreGenomeViewerRoot != null && herbivoreGenomeViewerRoot.activeSelf)
            BuildHerbivoreGenomeViewerList();
    }

    public void OnclickGenerationPageUp()
    {
        selectedGenerationPage = Mathf.Max(0, selectedGenerationPage + 1);
        if (herbivoreGenomeViewerRoot != null && herbivoreGenomeViewerRoot.activeSelf)
            BuildHerbivoreGenomeViewerList();
    }

    public void OnclickGenerationPageDown()
    {
        selectedGenerationPage = Mathf.Max(0, selectedGenerationPage - 1);
        if (herbivoreGenomeViewerRoot != null && herbivoreGenomeViewerRoot.activeSelf)
            BuildHerbivoreGenomeViewerList();
    }

    TMP_InputField CreateInputField(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string placeholderText)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(go.transform, false);
        var viewportRt = viewport.GetComponent<RectTransform>();
        viewportRt.anchorMin = new Vector2(0.02f, 0.08f);
        viewportRt.anchorMax = new Vector2(0.98f, 0.92f);
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;

        var text = CreateLabel("Text", viewport.transform, string.Empty, Vector2.zero, Vector2.one);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.TopLeft;

        var placeholder = CreateLabel("Placeholder", viewport.transform, placeholderText, Vector2.zero, Vector2.one);
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(1f, 1f, 1f, 0.4f);
        placeholder.alignment = TextAlignmentOptions.TopLeft;

        var input = go.GetComponent<TMP_InputField>();
        input.textViewport = viewportRt;
        input.textComponent = text;
        input.placeholder = placeholder;
        input.lineType = TMP_InputField.LineType.MultiLineNewline;
        return input;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.2f, 0.9f);

        var text = CreateLabel("Label", go.transform, label, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.88f));
        text.alignment = TextAlignmentOptions.Center;
        return go.GetComponent<Button>();
    }

    TextMeshProUGUI CreateLabel(string name, Transform parent, string textValue, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    public void AdvanceGeneration()
    {
        if (worldgen == null || resourceDispenser == null || herbivoreManager == null || predatorManager == null)
            return;

        var wg = worldgen.GetComponent<WorldGenerator>();
        if (wg == null || !wg.isgenerating)
            return;

        int herbivoreCount = Mathf.Max(1, CountExisting(herbivoreManager.herbivores));
        int predatorCount = Mathf.Max(1, CountExisting(predatorManager.predators));

        var rng = new System.Random(wg.seed + generationIndex * 7919);

        if (generationPhase == GenerationPhase.Both || generationPhase == GenerationPhase.HerbivoreOnly)
        {
            HerbivoreGenome nextHerbivoreGenome = ResolveHerbivoreGenome(rng);
            herbivoreManager.nextGenerationGenome = nextHerbivoreGenome;
            herbivoreManager.useManagerGenome = true;
            herbivoreManager.useNextGenerationGenome = true;
            if (saveOutputGenome)
                savedHerbivoreGenome = nextHerbivoreGenome;

            RecycleAndDestroyHerbivores();
            SpawnHerbivores(herbivoreCount);
        }

        if (generationPhase == GenerationPhase.Both || generationPhase == GenerationPhase.PredatorOnly)
        {
            PredatorGenome nextPredatorGenome = ResolvePredatorGenome(rng);
            predatorManager.nextGenerationGenome = nextPredatorGenome;
            predatorManager.useManagerGenome = true;
            predatorManager.useNextGenerationGenome = true;
            if (saveOutputGenome)
                savedPredatorGenome = nextPredatorGenome;

            RecycleAndDestroyPredators();
            SpawnPredators(predatorCount);
        }

        LogCurrentGeneration();
        generationIndex++;
    }

    public void onclickbutton2_1()
    {
        AdvanceGeneration();
    }

    public void onclickbutton2_2()
    {
        EnsureGenomeUi();
        bool show = herbivoreGenomeViewerRoot == null || !herbivoreGenomeViewerRoot.activeSelf;
        if (herbivoreGenomeViewerRoot != null)
            herbivoreGenomeViewerRoot.SetActive(show);

        if (herbivoreGenomeInjectorRoot != null && show)
            herbivoreGenomeInjectorRoot.SetActive(false);

        if (show)
            BuildHerbivoreGenomeViewerList();
    }

    public void onclickbutton2_3()
    {
        EnsureGenomeUi();
        bool show = herbivoreGenomeInjectorRoot == null || !herbivoreGenomeInjectorRoot.activeSelf;
        if (herbivoreGenomeInjectorRoot != null)
            herbivoreGenomeInjectorRoot.SetActive(show);

        if (herbivoreGenomeViewerRoot != null && show)
            herbivoreGenomeViewerRoot.SetActive(false);
    }

    public void OnclickSpawnHerbivoreGenome()
    {
        EnsureGenomeUi();
        string dna = herbivoreGenomeInjectorInput != null ? herbivoreGenomeInjectorInput.text : string.Empty;
        if (string.IsNullOrWhiteSpace(dna))
        {
            SetHerbivoreGenomeUiStatus("DNA code is empty.");
            return;
        }

        if (worldgen == null || resourceDispenser == null || herbivoreManager == null)
        {
            SetHerbivoreGenomeUiStatus("Reference is missing.");
            return;
        }

        try
        {
            HerbivoreGenome genome = GenomeSerializer.DecodeGenome(dna);
            if (!herbivoreManager.SpawnHerbivoreWithGenome(worldgen, injectedHerbivoreSpawnIndex++, genome, out GameObject herbivore))
            {
                SetHerbivoreGenomeUiStatus("Spawn failed.");
                return;
            }

            resourceDispenser.InitializeCreatureResource(herbivore, resourceDispenser.carbonPerHerbivore, category.herbivore);
            SetHerbivoreGenomeUiStatus("Spawned herbivore from DNA.");

            if (herbivoreGenomeViewerRoot != null && herbivoreGenomeViewerRoot.activeSelf)
                BuildHerbivoreGenomeViewerList();
        }
        catch (Exception ex)
        {
            SetHerbivoreGenomeUiStatus("Invalid DNA: " + ex.Message);
        }
    }

    public void OnclickSpawnGenome()
    {
        OnclickSpawnHerbivoreGenome();
    }

    void BuildHerbivoreGenomeViewerList()
    {
        EnsureGenomeUi();
        ClearHerbivoreGenomeViewerList();

        if (herbivoreGenomeViewerContent == null)
        {
            SetHerbivoreGenomeUiStatus("Genome list UI reference is missing.");
            return;
        }

        int start = selectedGenerationPage * generationPageSize;

        if (SelectedSpecies == GenomeViewerItemEntry.SpeciesType.Herbivore)
        {
            if (herbivoreManager == null)
            {
                SetHerbivoreGenomeUiStatus("Herbivore manager reference is missing.");
                return;
            }

            int endExclusive = Mathf.Min(herbivoreManager.herbivores.Count, start + generationPageSize);
            for (int i = start; i < endExclusive; i++)
            {
                GameObject obj = herbivoreManager.herbivores[i];
                if (obj == null) continue;
                if (!obj.TryGetComponent<herbivoreBehaviour>(out var hb)) continue;

                string encoded = GenomeSerializer.EncodeGenome(hb.genome);
                AddGenomeViewerItem(encoded, SelectedSpecies, SelectedPhaseIndex, i, hb.IsDead);
            }
        }
        else
        {
            if (predatorManager == null)
            {
                SetHerbivoreGenomeUiStatus("Predator manager reference is missing.");
                return;
            }

            int endExclusive = Mathf.Min(predatorManager.predators.Count, start + generationPageSize);
            for (int i = start; i < endExclusive; i++)
            {
                GameObject obj = predatorManager.predators[i];
                if (obj == null) continue;
                if (!obj.TryGetComponent<predatorBehaviour>(out var pb)) continue;

                string encoded = EncodePredatorGenome(pb.genome);
                AddGenomeViewerItem(encoded, SelectedSpecies, SelectedPhaseIndex, i, pb.IsDead);
            }
        }

        if (herbivoreGenomePageStatusText != null)
            herbivoreGenomePageStatusText.text = $"species:{SelectedSpecies} phase:{SelectedPhaseIndex} page:{selectedGenerationPage}";
    }

    void ClearHerbivoreGenomeViewerList()
    {
        for (int i = 0; i < createdGenomeUi.Count; i++)
        {
            if (createdGenomeUi[i] != null)
                Destroy(createdGenomeUi[i]);
        }
        createdGenomeUi.Clear();
    }

    void AddGenomeViewerItem(string dna, GenomeViewerItemEntry.SpeciesType species, int phaseId, int numberId, bool isDead)
    {
        EnsureGenomeViewerItemPrefab();
        GameObject item = Instantiate(herbivoreGenomeViewerItemPrefab, herbivoreGenomeViewerContent);
        item.SetActive(true);
        createdGenomeUi.Add(item);

        var entry = item.GetComponent<GenomeViewerItemEntry>();
        if (entry != null)
        {
            entry.Initialize(
                this,
                herbivoreManager,
                predatorManager,
                species,
                phaseId,
                numberId,
                dna,
                isDead);
        }
    }

    public void NotifyGenomeUiStatus(string message)
    {
        SetHerbivoreGenomeUiStatus(message);
    }

    void SetHerbivoreGenomeUiStatus(string message)
    {
        if (herbivoreGenomeUiStatusText != null)
            herbivoreGenomeUiStatusText.text = message;
    }

    void LogCurrentGeneration()
    {
        if (herbivoreManager == null)
            return;

        int population = 0;
        bool foundBest = false;
        float bestFitness = 0f;
        HerbivoreGenome bestGenome = default;
        var rng = new System.Random(GetBaseSeed() + generationIndex * 1223);

        for (int i = 0; i < herbivoreManager.herbivores.Count; i++)
        {
            GameObject obj = herbivoreManager.herbivores[i];
            if (obj == null) continue;
            if (!obj.TryGetComponent<herbivoreBehaviour>(out var behaviour)) continue;
            if (!obj.TryGetComponent<Resource>(out var resource)) continue;

            population++;
            float fitness = EvaluateScore(resource.carbon, behaviour.health, rng);
            if (!foundBest || fitness > bestFitness)
            {
                foundBest = true;
                bestFitness = fitness;
                bestGenome = behaviour.genome;
            }
        }

        GenerationLog log = new GenerationLog
        {
            generation = generationIndex,
            timestamp = DateTime.UtcNow.ToString("o"),
            population = population,
            bestGenome = foundBest ? GenomeSerializer.EncodeGenome(bestGenome) : string.Empty,
            bestFitness = foundBest ? bestFitness : 0f
        };

        GenomeLogger.AppendLog(log);
    }

    public void SaveCurrentGenomes()
    {
        int baseSeed = GetBaseSeed();
        savedHerbivoreGenome = ResolveHerbivoreGenome(new System.Random(baseSeed));
        savedPredatorGenome = ResolvePredatorGenome(new System.Random(baseSeed + 1));
    }

    public void ApplySavedGenomes()
    {
        herbivoreManager.nextGenerationGenome = savedHerbivoreGenome;
        herbivoreManager.useManagerGenome = true;
        herbivoreManager.useNextGenerationGenome = true;

        predatorManager.nextGenerationGenome = savedPredatorGenome;
        predatorManager.useManagerGenome = true;
        predatorManager.useNextGenerationGenome = true;
    }

    int CountExisting(List<GameObject> objects)
    {
        int count = 0;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
                count++;
        }
        return count;
    }

    int GetBaseSeed()
    {
        if (worldgen != null && worldgen.TryGetComponent<WorldGenerator>(out var wg))
            return wg.seed;

        return generationIndex;
    }

    void RecycleAndDestroyHerbivores()
    {
        for (int i = herbivoreManager.herbivores.Count - 1; i >= 0; i--)
        {
            GameObject obj = herbivoreManager.herbivores[i];
            if (obj == null) continue;

            ReturnCarbonToEnvironment(obj);
            Destroy(obj);
        }
        herbivoreManager.herbivores.Clear();
    }

    void RecycleAndDestroyPredators()
    {
        for (int i = predatorManager.predators.Count - 1; i >= 0; i--)
        {
            GameObject obj = predatorManager.predators[i];
            if (obj == null) continue;

            ReturnCarbonToEnvironment(obj);
            Destroy(obj);
        }
        predatorManager.predators.Clear();
    }

    void ReturnCarbonToEnvironment(GameObject obj)
    {
        if (obj != null && obj.TryGetComponent<Resource>(out var resource))
        {
            resourceDispenser.ReturnCarbon(resource.carbon);
        }
    }

    void SpawnHerbivores(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (herbivoreManager.spownherbivore(worldgen, generationIndex * 1000 + i, out GameObject herbivore))
            {
                resourceDispenser.InitializeCreatureResource(herbivore, resourceDispenser.carbonPerHerbivore, category.herbivore);
            }
        }
    }

    void SpawnPredators(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (predatorManager.spownpredator(worldgen, generationIndex * 1000 + i, out GameObject predator))
            {
                resourceDispenser.InitializeCreatureResource(predator, resourceDispenser.carbonPerPredator, category.predator);
            }
        }
    }

    HerbivoreGenome ResolveHerbivoreGenome(System.Random rng)
    {
        if (evaluationAxis == EvaluationAxis.Selection &&
            TryResolveHerbivoreGenomeFromBuckets(rng, out HerbivoreGenome selectedHerbivoreGenome))
            return selectedHerbivoreGenome;

        if (herbivoreInputMode == GenomeInputMode.SavedGenome)
            return MaybeMutate(Breed(savedHerbivoreGenome, savedHerbivoreGenome, rng), savedHerbivoreGenome, savedHerbivoreGenome, rng);

        if (herbivoreInputMode == GenomeInputMode.ManagerGenome)
        {
            HerbivoreGenome managerGenome = herbivoreManager.useNextGenerationGenome ? herbivoreManager.nextGenerationGenome : herbivoreManager.genome;
            return MaybeMutate(Breed(managerGenome, managerGenome, rng), managerGenome, managerGenome, rng);
        }

        List<HerbivoreCandidate> candidates = CollectHerbivoreCandidates(rng);
        if (candidates.Count == 0)
        {
            HerbivoreGenome fallback = herbivoreManager.useNextGenerationGenome ? herbivoreManager.nextGenerationGenome : herbivoreManager.genome;
            return MaybeMutate(Breed(fallback, fallback, rng), fallback, fallback, rng);
        }

        candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
        HerbivoreGenome parentA = candidates[0].Genome;
        HerbivoreGenome parentB = candidates.Count > 1 ? candidates[1].Genome : parentA;
        HerbivoreGenome child = Breed(parentA, parentB, rng);
        return MaybeMutate(child, parentA, parentB, rng);
    }

    PredatorGenome ResolvePredatorGenome(System.Random rng)
    {
        if (evaluationAxis == EvaluationAxis.Selection &&
            TryResolvePredatorGenomeFromBuckets(rng, out PredatorGenome selectedPredatorGenome))
            return selectedPredatorGenome;

        if (predatorInputMode == GenomeInputMode.SavedGenome)
            return MaybeMutate(Breed(savedPredatorGenome, savedPredatorGenome, rng), savedPredatorGenome, savedPredatorGenome, rng);

        if (predatorInputMode == GenomeInputMode.ManagerGenome)
        {
            PredatorGenome managerGenome = predatorManager.useNextGenerationGenome ? predatorManager.nextGenerationGenome : predatorManager.genome;
            return MaybeMutate(Breed(managerGenome, managerGenome, rng), managerGenome, managerGenome, rng);
        }

        List<PredatorCandidate> candidates = CollectPredatorCandidates(rng);
        if (candidates.Count == 0)
        {
            PredatorGenome fallback = predatorManager.useNextGenerationGenome ? predatorManager.nextGenerationGenome : predatorManager.genome;
            return MaybeMutate(Breed(fallback, fallback, rng), fallback, fallback, rng);
        }

        candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
        PredatorGenome parentA = candidates[0].Genome;
        PredatorGenome parentB = candidates.Count > 1 ? candidates[1].Genome : parentA;
        PredatorGenome child = Breed(parentA, parentB, rng);
        return MaybeMutate(child, parentA, parentB, rng);
    }

    bool TryResolveHerbivoreGenomeFromBuckets(System.Random rng, out HerbivoreGenome genome)
    {
        genome = default;
        if (herbivoreManager == null || herbivoreManager.genomes == null || herbivoreManager.genomes.Count == 0)
            return false;

        List<string> codes = CollectPhaseDnaCodes(herbivoreManager.genomes, SelectedPhaseIndex);
        codes = SliceByGenerationPage(codes, selectedGenerationPage, generationPageSize);
        if (codes.Count == 0)
            return false;

        var pool = new List<HerbivoreGenome>();
        for (int i = 0; i < codes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(codes[i])) continue;
            try
            {
                pool.Add(GenomeSerializer.DecodeGenome(codes[i]));
            }
            catch
            {
            }
        }

        if (pool.Count == 0)
            return false;

        HerbivoreGenome parentA = pool[rng.Next(pool.Count)];
        HerbivoreGenome parentB = pool[rng.Next(pool.Count)];
        genome = MaybeMutate(Breed(parentA, parentB, rng), parentA, parentB, rng);
        return true;
    }

    bool TryResolvePredatorGenomeFromBuckets(System.Random rng, out PredatorGenome genome)
    {
        genome = default;
        if (predatorManager == null || predatorManager.genomes == null || predatorManager.genomes.Count == 0)
            return false;

        List<string> codes = CollectPhaseDnaCodes(predatorManager.genomes, SelectedPhaseIndex);
        codes = SliceByGenerationPage(codes, selectedGenerationPage, generationPageSize);
        if (codes.Count == 0)
            return false;

        var pool = new List<PredatorGenome>();
        for (int i = 0; i < codes.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(codes[i])) continue;
            if (TryDecodePredatorGenome(codes[i], out PredatorGenome decoded))
                pool.Add(decoded);
        }

        if (pool.Count == 0)
            return false;

        PredatorGenome parentA = pool[rng.Next(pool.Count)];
        PredatorGenome parentB = pool[rng.Next(pool.Count)];
        genome = MaybeMutate(Breed(parentA, parentB, rng), parentA, parentB, rng);
        return true;
    }

    List<string> CollectPhaseDnaCodes(List<GenomePhaseBucket> buckets, int phaseIndex)
    {
        var list = new List<string>();
        if (buckets == null || buckets.Count == 0)
            return list;

        if (phaseIndex >= 0 && phaseIndex < buckets.Count && buckets[phaseIndex] != null)
        {
            list.AddRange(buckets[phaseIndex].dnaCodes);
            return list;
        }

        for (int i = 0; i < buckets.Count; i++)
        {
            if (buckets[i] == null) continue;
            list.AddRange(buckets[i].dnaCodes);
        }

        return list;
    }

    List<string> SliceByGenerationPage(List<string> codes, int page, int pageSize)
    {
        if (codes == null || codes.Count == 0)
            return new List<string>();

        int safePage = Mathf.Max(0, page);
        int start = safePage * Mathf.Max(1, pageSize);
        if (start >= codes.Count)
            return new List<string>();

        int count = Mathf.Min(pageSize, codes.Count - start);
        return codes.GetRange(start, count);
    }

    bool TryDecodePredatorGenome(string text, out PredatorGenome genome)
    {
        genome = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        const string predatorPrefix = "PGJ:";
        string raw = text.Trim();
        if (raw.StartsWith(predatorPrefix, StringComparison.Ordinal))
            raw = raw.Substring(predatorPrefix.Length);

        try
        {
            genome = JsonUtility.FromJson<PredatorGenome>(raw);
            return true;
        }
        catch
        {
            return false;
        }
    }

    string EncodePredatorGenome(PredatorGenome genome)
    {
        return "PGJ:" + JsonUtility.ToJson(genome);
    }

    List<HerbivoreCandidate> CollectHerbivoreCandidates(System.Random rng)
    {
        var candidates = new List<HerbivoreCandidate>();
        for (int i = 0; i < herbivoreManager.herbivores.Count; i++)
        {
            GameObject obj = herbivoreManager.herbivores[i];
            if (obj == null) continue;
            if (!obj.TryGetComponent<herbivoreBehaviour>(out var behaviour)) continue;
            if (!obj.TryGetComponent<Resource>(out var resource)) continue;

            candidates.Add(new HerbivoreCandidate
            {
                Genome = behaviour.genome,
                Score = EvaluateScore(resource.carbon, behaviour.health, rng)
            });
        }
        return candidates;
    }

    List<PredatorCandidate> CollectPredatorCandidates(System.Random rng)
    {
        var candidates = new List<PredatorCandidate>();
        for (int i = 0; i < predatorManager.predators.Count; i++)
        {
            GameObject obj = predatorManager.predators[i];
            if (obj == null) continue;
            if (!obj.TryGetComponent<predatorBehaviour>(out var behaviour)) continue;
            if (!obj.TryGetComponent<Resource>(out var resource)) continue;

            candidates.Add(new PredatorCandidate
            {
                Genome = behaviour.genome,
                Score = EvaluateScore(resource.carbon, behaviour.health, rng)
            });
        }
        return candidates;
    }

    float EvaluateScore(float carbon, float health, System.Random rng)
    {
        switch (evaluationAxis)
        {
            case EvaluationAxis.Carbon:
                return carbon;
            case EvaluationAxis.Health:
                return health;
            default:
                return (float)rng.NextDouble();
        }
    }

    HerbivoreGenome Breed(HerbivoreGenome a, HerbivoreGenome b, System.Random rng)
    {
        if (!enableCrossover)
            return a;

        HerbivoreGenome child = new HerbivoreGenome
        {
            forwardForce = BlendFloat(a.forwardForce, b.forwardForce, rng),
            turnForce = BlendFloat(a.turnForce, b.turnForce, rng),
            visionAngle = BlendFloat(a.visionAngle, b.visionAngle, rng),
            visionturnAngle = BlendFloat(a.visionturnAngle, b.visionturnAngle, rng),
            visionDistance = BlendFloat(a.visionDistance, b.visionDistance, rng),
            metabolismRate = BlendFloat(a.metabolismRate, b.metabolismRate, rng),
            eatspeed = BlendFloat(a.eatspeed, b.eatspeed, rng),
            threatWeight = BlendFloat(a.threatWeight, b.threatWeight, rng),
            threatDetectDistance = BlendFloat(a.threatDetectDistance, b.threatDetectDistance, rng),
            memorytime = BlendFloat(a.memorytime, b.memorytime, rng),
            runAwayDistance = BlendFloat(a.runAwayDistance, b.runAwayDistance, rng),
            contactEscapeDistance = BlendFloat(a.contactEscapeDistance, b.contactEscapeDistance, rng),
            evasionAngle = BlendFloat(a.evasionAngle, b.evasionAngle, rng),
            evasionDuration = BlendFloat(a.evasionDuration, b.evasionDuration, rng),
            evasionCooldown = BlendFloat(a.evasionCooldown, b.evasionCooldown, rng),
            evasionDistance = BlendFloat(a.evasionDistance, b.evasionDistance, rng),
            predictIntercept = rng.NextDouble() < 0.5 ? a.predictIntercept : b.predictIntercept,
            zigzagFrequency = BlendFloat(a.zigzagFrequency, b.zigzagFrequency, rng),
            zigzagAmplitude = BlendFloat(a.zigzagAmplitude, b.zigzagAmplitude, rng),
            foodWeight = BlendFloat(a.foodWeight, b.foodWeight, rng),
            predatorWeight = BlendFloat(a.predatorWeight, b.predatorWeight, rng),
            corpseWeight = BlendFloat(a.corpseWeight, b.corpseWeight, rng),
            fearThreshold = BlendFloat(a.fearThreshold, b.fearThreshold, rng),
            escapeThreshold = BlendFloat(a.escapeThreshold, b.escapeThreshold, rng),
            curiosity = BlendFloat(a.curiosity, b.curiosity, rng),
            visionWaves = BlendWaves(a.visionWaves, b.visionWaves, rng),
            wanderWaves = BlendWaves(a.wanderWaves, b.wanderWaves, rng)
        };
        return child;
    }

    PredatorGenome Breed(PredatorGenome a, PredatorGenome b, System.Random rng)
    {
        if (!enableCrossover)
            return a;

        PredatorGenome child = new PredatorGenome
        {
            forwardForce = BlendFloat(a.forwardForce, b.forwardForce, rng),
            turnForce = BlendFloat(a.turnForce, b.turnForce, rng),
            visionAngle = BlendFloat(a.visionAngle, b.visionAngle, rng),
            visionTurnAngle = BlendFloat(a.visionTurnAngle, b.visionTurnAngle, rng),
            visionDistance = BlendFloat(a.visionDistance, b.visionDistance, rng),
            metabolismRate = BlendFloat(a.metabolismRate, b.metabolismRate, rng),
            eatspeed = BlendFloat(a.eatspeed, b.eatspeed, rng),
            chaseWeight = BlendFloat(a.chaseWeight, b.chaseWeight, rng),
            preyDetectDistance = BlendFloat(a.preyDetectDistance, b.preyDetectDistance, rng),
            attackDistance = BlendFloat(a.attackDistance, b.attackDistance, rng),
            attackDamage = BlendFloat(a.attackDamage, b.attackDamage, rng),
            attackCooldown = BlendFloat(a.attackCooldown, b.attackCooldown, rng),
            threatWeight = BlendFloat(a.threatWeight, b.threatWeight, rng),
            threatDetectDistance = BlendFloat(a.threatDetectDistance, b.threatDetectDistance, rng),
            memorytime = BlendFloat(a.memorytime, b.memorytime, rng),
            preferredChaseDistance = BlendFloat(a.preferredChaseDistance, b.preferredChaseDistance, rng),
            disengageDistance = BlendFloat(a.disengageDistance, b.disengageDistance, rng),
            visionWaves = BlendWaves(a.visionWaves, b.visionWaves, rng),
            wanderWaves = BlendWaves(a.wanderWaves, b.wanderWaves, rng)
        };
        return child;
    }

    HerbivoreGenome MaybeMutate(HerbivoreGenome child, HerbivoreGenome a, HerbivoreGenome b, System.Random rng)
    {
        if (!enableMutation) return child;

        child.forwardForce = MutateFloat(child.forwardForce, a.forwardForce, b.forwardForce, rng);
        child.turnForce = MutateFloat(child.turnForce, a.turnForce, b.turnForce, rng);
        child.visionAngle = MutateFloat(child.visionAngle, a.visionAngle, b.visionAngle, rng);
        child.visionturnAngle = MutateFloat(child.visionturnAngle, a.visionturnAngle, b.visionturnAngle, rng);
        child.visionDistance = MutateFloat(child.visionDistance, a.visionDistance, b.visionDistance, rng);
        child.metabolismRate = MutateFloat(child.metabolismRate, a.metabolismRate, b.metabolismRate, rng);
        child.eatspeed = MutateFloat(child.eatspeed, a.eatspeed, b.eatspeed, rng);
        child.threatWeight = MutateFloat(child.threatWeight, a.threatWeight, b.threatWeight, rng);
        child.threatDetectDistance = MutateFloat(child.threatDetectDistance, a.threatDetectDistance, b.threatDetectDistance, rng);
        child.memorytime = MutateFloat(child.memorytime, a.memorytime, b.memorytime, rng);
        child.runAwayDistance = MutateFloat(child.runAwayDistance, a.runAwayDistance, b.runAwayDistance, rng);
        child.contactEscapeDistance = MutateFloat(child.contactEscapeDistance, a.contactEscapeDistance, b.contactEscapeDistance, rng);
        child.evasionAngle = MutateFloat(child.evasionAngle, a.evasionAngle, b.evasionAngle, rng);
        child.evasionDuration = MutateFloat(child.evasionDuration, a.evasionDuration, b.evasionDuration, rng);
        child.evasionCooldown = MutateFloat(child.evasionCooldown, a.evasionCooldown, b.evasionCooldown, rng);
        child.evasionDistance = MutateFloat(child.evasionDistance, a.evasionDistance, b.evasionDistance, rng);
        child.zigzagFrequency = MutateFloat(child.zigzagFrequency, a.zigzagFrequency, b.zigzagFrequency, rng);
        child.zigzagAmplitude = MutateFloat(child.zigzagAmplitude, a.zigzagAmplitude, b.zigzagAmplitude, rng);
        child.foodWeight = MutateFloat(child.foodWeight, a.foodWeight, b.foodWeight, rng);
        child.predatorWeight = MutateFloat(child.predatorWeight, a.predatorWeight, b.predatorWeight, rng);
        child.corpseWeight = MutateFloat(child.corpseWeight, a.corpseWeight, b.corpseWeight, rng);
        child.fearThreshold = MutateFloat(child.fearThreshold, a.fearThreshold, b.fearThreshold, rng);
        child.escapeThreshold = MutateFloat(child.escapeThreshold, a.escapeThreshold, b.escapeThreshold, rng);
        child.curiosity = MutateFloat(child.curiosity, a.curiosity, b.curiosity, rng);
        child.visionWaves = MutateWaves(child.visionWaves, a.visionWaves, b.visionWaves, rng);
        child.wanderWaves = MutateWaves(child.wanderWaves, a.wanderWaves, b.wanderWaves, rng);
        return child;
    }

    PredatorGenome MaybeMutate(PredatorGenome child, PredatorGenome a, PredatorGenome b, System.Random rng)
    {
        if (!enableMutation) return child;

        child.forwardForce = MutateFloat(child.forwardForce, a.forwardForce, b.forwardForce, rng);
        child.turnForce = MutateFloat(child.turnForce, a.turnForce, b.turnForce, rng);
        child.visionAngle = MutateFloat(child.visionAngle, a.visionAngle, b.visionAngle, rng);
        child.visionTurnAngle = MutateFloat(child.visionTurnAngle, a.visionTurnAngle, b.visionTurnAngle, rng);
        child.visionDistance = MutateFloat(child.visionDistance, a.visionDistance, b.visionDistance, rng);
        child.metabolismRate = MutateFloat(child.metabolismRate, a.metabolismRate, b.metabolismRate, rng);
        child.eatspeed = MutateFloat(child.eatspeed, a.eatspeed, b.eatspeed, rng);
        child.chaseWeight = MutateFloat(child.chaseWeight, a.chaseWeight, b.chaseWeight, rng);
        child.preyDetectDistance = MutateFloat(child.preyDetectDistance, a.preyDetectDistance, b.preyDetectDistance, rng);
        child.attackDistance = MutateFloat(child.attackDistance, a.attackDistance, b.attackDistance, rng);
        child.attackDamage = MutateFloat(child.attackDamage, a.attackDamage, b.attackDamage, rng);
        child.attackCooldown = MutateFloat(child.attackCooldown, a.attackCooldown, b.attackCooldown, rng);
        child.threatWeight = MutateFloat(child.threatWeight, a.threatWeight, b.threatWeight, rng);
        child.threatDetectDistance = MutateFloat(child.threatDetectDistance, a.threatDetectDistance, b.threatDetectDistance, rng);
        child.memorytime = MutateFloat(child.memorytime, a.memorytime, b.memorytime, rng);
        child.preferredChaseDistance = MutateFloat(child.preferredChaseDistance, a.preferredChaseDistance, b.preferredChaseDistance, rng);
        child.disengageDistance = MutateFloat(child.disengageDistance, a.disengageDistance, b.disengageDistance, rng);
        child.visionWaves = MutateWaves(child.visionWaves, a.visionWaves, b.visionWaves, rng);
        child.wanderWaves = MutateWaves(child.wanderWaves, a.wanderWaves, b.wanderWaves, rng);
        return child;
    }

    float BlendFloat(float a, float b, System.Random rng)
    {
        switch (crossoverMode)
        {
            case CrossoverMode.Assign:
                return rng.NextDouble() < 0.5 ? a : b;
            case CrossoverMode.Average:
                return (a + b) * 0.5f;
            case CrossoverMode.Interpolate:
                return Mathf.Lerp(a, b, (float)rng.NextDouble());
            default:
                return rng.NextDouble() < 0.5 ? a : b;
        }
    }

    WaveGene[] BlendWaves(WaveGene[] a, WaveGene[] b, System.Random rng)
    {
        int countA = a != null ? a.Length : 0;
        int countB = b != null ? b.Length : 0;
        int count = Mathf.Max(1, Mathf.Max(countA, countB));

        var result = new WaveGene[count];
        for (int i = 0; i < count; i++)
        {
            WaveGene wa = countA > 0 ? a[Mathf.Min(i, countA - 1)] : default;
            WaveGene wb = countB > 0 ? b[Mathf.Min(i, countB - 1)] : default;
            result[i] = new WaveGene
            {
                frequency = BlendFloat(wa.frequency, wb.frequency, rng),
                amplitude = BlendFloat(wa.amplitude, wb.amplitude, rng),
                phase = BlendFloat(wa.phase, wb.phase, rng)
            };
        }
        return result;
    }

    float MutateFloat(float value, float parentA, float parentB, System.Random rng)
    {
        if (rng.NextDouble() > mutationChance)
            return value;

        if (mutationRangeMode == MutationRangeMode.GlobalRange)
        {
            return value + Mathf.Lerp(globalMutationMin, globalMutationMax, (float)rng.NextDouble());
        }

        float parentBase = Mathf.Max(0.01f, Mathf.Abs((parentA + parentB) * 0.5f));
        float delta = Mathf.Lerp(-parentBase * parentMutationScale, parentBase * parentMutationScale, (float)rng.NextDouble());
        return value + delta;
    }

    WaveGene[] MutateWaves(WaveGene[] waves, WaveGene[] parentA, WaveGene[] parentB, System.Random rng)
    {
        if (waves == null || waves.Length == 0)
            return waves;

        for (int i = 0; i < waves.Length; i++)
        {
            WaveGene pa = parentA != null && parentA.Length > 0 ? parentA[Mathf.Min(i, parentA.Length - 1)] : default;
            WaveGene pb = parentB != null && parentB.Length > 0 ? parentB[Mathf.Min(i, parentB.Length - 1)] : default;
            WaveGene current = waves[i];
            current.frequency = MutateFloat(current.frequency, pa.frequency, pb.frequency, rng);
            current.amplitude = MutateFloat(current.amplitude, pa.amplitude, pb.amplitude, rng);
            current.phase = MutateFloat(current.phase, pa.phase, pb.phase, rng);
            waves[i] = current;
        }
        return waves;
    }

    struct HerbivoreCandidate
    {
        public HerbivoreGenome Genome;
        public float Score;
    }

    struct PredatorCandidate
    {
        public PredatorGenome Genome;
        public float Score;
    }
}

[Serializable]
public class GenomePhaseBucket
{
    public List<string> dnaCodes = new List<string>();
}

public class GenomeViewerItemEntry : MonoBehaviour
{
    public enum SpeciesType
    {
        Herbivore,
        Predator
    }

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
