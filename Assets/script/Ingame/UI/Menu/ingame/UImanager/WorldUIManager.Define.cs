using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class WorldUIManager
{
    [Header("Debug Toggle")]
    [FormerlySerializedAs("showWorldMenu")]
    [SerializeField] bool isWorldMenuVisible = false;
    [FormerlySerializedAs("showObjectList")]
    [SerializeField] bool isObjectListVisible = false;
    [FormerlySerializedAs("showStatus")]
    [SerializeField] bool isStatusVisible = false;

    [Header("UI Branch References")]
    [FormerlySerializedAs("Menu_root_0")]
    [SerializeField] UITreeBranch menuRootBranch;
    [FormerlySerializedAs("ObjectList_tab_00")]
    [SerializeField] UITreeBranch objectListBranch;
    [FormerlySerializedAs("GenController_tab_01")]
    [SerializeField] UITreeBranch generationBranch;
    [FormerlySerializedAs("Properties_tab_02")]
    [SerializeField] UITreeBranch propertiesBranch;
    [FormerlySerializedAs("AdvanceGeneration_branch_012")]
    [SerializeField] UITreeBranch advanceGenerationBranch;
    [FormerlySerializedAs("LogView_branch_010")]
    [SerializeField] UITreeBranch genomeViewerBranch;
    [FormerlySerializedAs("GenomeInjector_branch_011")]
    [SerializeField] UITreeBranch genomeInjectorBranch;
    [FormerlySerializedAs("Detail_leaf_00x0")]
    [SerializeField] UITreeBranch detailBranch;
    [FormerlySerializedAs("StateViewPageDown_leaf_00x1")]
    [SerializeField] UITreeBranch stateViewPageDownBranch;
    [FormerlySerializedAs("StateViewPageUp_leaf_00x2")]
    [SerializeField] UITreeBranch stateViewPageUpBranch;
    [SerializeField] UITreeBranch fieldViewingTab;
    [SerializeField] UITreeBranch fieldViewingDropdownBranch;
    [SerializeField] UITreeBranch disturbanceTab;
    [SerializeField] UITreeBranch disturbanceElementBranch;
    [SerializeField] GameObject viewDisplayFoundationRoot;
    [SerializeField] GameObject uiFreqRoot;

    [Header("References")]
    public Canvas mainCanvas;
    public Camera mainCamera;

    [Header("UI")]
    [SerializeField] GameObject text_follow;
    TextMeshProUGUI text_f;

    [Header("Manager State")]
    public GameObject grassManager;
    public GameObject herbivoreManager;
    public GameObject predatorManager;
    public AdvanceGenerationController generationController;

    GameObject currentTarget;
    public bool RotationThenlooking = false;

    [SerializeField] LayerMask selectableLayer;

    [SerializeField] GameObject viewDisplayFoundation;
    [SerializeField] RawImage waveImage;
    [SerializeField] TextMeshProUGUI UI_freq;

    Texture2D waveTexture;
    Color32[] pixelBuffer;
    int stateViewPage = 0;
    const int stateViewPageCount = 4;
    string currentHerbivoreDnaCode = string.Empty;
    Button herbivoreDnaCopyButton;
    Button menuRootButton;
    int lastMenuInvokeFrame = -1;
    int lastObjectListInvokeFrame = -1;
    int lastGenerationInvokeFrame = -1;
    int lastAdvanceGenerationInvokeFrame = -1;
    int lastGenomeViewerInvokeFrame = -1;
    int lastGenomeInjectorInvokeFrame = -1;
    int lastPropertiesInvokeFrame = -1;
    int lastStateInvokeFrame = -1;
    int lastPageDownInvokeFrame = -1;
    int lastPageUpInvokeFrame = -1;
    int lastFieldViewingInvokeFrame = -1;
    int lastDisturbanceInvokeFrame = -1;
    bool IsStateViewVisible => isStatusVisible && isObjectListVisible;
}
