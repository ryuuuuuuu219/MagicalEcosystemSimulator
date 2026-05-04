using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class WorldUIManager
{
    void EnsureSceneButtonBindings()
    {
        menuRootButton = GetButton(menuRootBranch);

        EnsureMenuButtonRelay(menuRootButton);
        BindSceneButton(GetButton(objectListBranch), Onclickbutton1);
        BindSceneButton(GetButton(generationBranch), Onclickbutton2);
        BindSceneButton(GetButton(detailBranch), Onclick_State);
        BindSceneButton(GetButton(stateViewPageDownBranch), Onclick_PageDown);
        BindSceneButton(GetButton(stateViewPageUpBranch), Onclick_PageUp);
        BindSceneButton(GetButton(advanceGenerationBranch), Onclickbutton2_1);
        BindSceneButton(GetButton(genomeViewerBranch), Onclickbutton2_2);
        BindSceneButton(GetButton(genomeInjectorBranch), Onclickbutton2_3);
        BindSceneButton(GetButton(propertiesBranch), Onclickbutton3);
        BindSceneButton(GetButton(fieldViewingTab), OnclickFieldViewing);
        BindSceneButton(GetButton(disturbanceTab), OnclickDisturbance);
    }

    public void Menu()
    {
        if (IsDuplicateUiInvoke(ref lastMenuInvokeFrame))
            return;

        isWorldMenuVisible = !isWorldMenuVisible;
        if (isWorldMenuVisible)
        {
            ShowMenuRootButtons();
            return;
        }

        isObjectListVisible = false;
        isStatusVisible = false;
        currentTarget = null;

        HideAllMenuBranches();
        ClearObjectList();
        HideStatusUI();
        ClearStateview();
        UpdateFollowText();
    }

    public void Onclickbutton1()
    {
        if (!IsDuplicateUiInvoke(ref lastObjectListInvokeFrame))
            ToggleObjectListBranch();
    }

    public void Onclickbutton2()
    {
        if (!IsDuplicateUiInvoke(ref lastGenerationInvokeFrame))
            ToggleGenerationBranch();
    }

    public void Onclickbutton2_1()
    {
        if (!IsDuplicateUiInvoke(ref lastAdvanceGenerationInvokeFrame))
            OpenAdvanceGenerationBranch();
    }

    public void Onclickbutton2_2()
    {
        if (!IsDuplicateUiInvoke(ref lastGenomeViewerInvokeFrame))
            ToggleGenomeViewerBranch();
    }

    public void Onclickbutton2_3()
    {
        if (!IsDuplicateUiInvoke(ref lastGenomeInjectorInvokeFrame))
            ToggleGenomeInjectorBranch();
    }

    public void Onclickbutton3()
    {
        if (!IsDuplicateUiInvoke(ref lastPropertiesInvokeFrame))
            TogglePropertiesBranch();
    }

    public void OnclickFieldViewing()
    {
        if (!IsDuplicateUiInvoke(ref lastFieldViewingInvokeFrame))
            ToggleFieldViewingBranch();
    }

    public void OnclickDisturbance()
    {
        if (!IsDuplicateUiInvoke(ref lastDisturbanceInvokeFrame))
            ToggleDisturbanceBranch();
    }

    IEnumerable<GameObject> EnumerateInitialUiObjects()
    {
        yield return GetBranchObject(objectListBranch);
        yield return GetBranchObject(generationBranch);
        yield return GetBranchObject(propertiesBranch);
        yield return GetBranchObject(detailBranch);
        yield return GetBranchObject(stateViewPageDownBranch);
        yield return GetBranchObject(stateViewPageUpBranch);
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
        yield return GetBranchObject(genomeViewerBranch);
        yield return GetBranchObject(genomeInjectorBranch);
        yield return GetBranchObject(advanceGenerationBranch);
        yield return GetBranchObject(fieldViewingTab);
        yield return GetBranchObject(fieldViewingDropdownBranch);
        yield return GetBranchObject(disturbanceTab);
        yield return GetBranchObject(disturbanceElementBranch);
    }

    IEnumerable<GameObject> EnumerateStatusButtons()
    {
        yield return GetBranchObject(detailBranch);
        yield return GetBranchObject(stateViewPageDownBranch);
        yield return GetBranchObject(stateViewPageUpBranch);
    }

    IEnumerable<GameObject> EnumerateStatusInfoObjects()
    {
        yield return viewDisplayFoundationRoot != null ? viewDisplayFoundationRoot : viewDisplayFoundation;
        yield return uiFreqRoot != null ? uiFreqRoot : (UI_freq != null ? UI_freq.gameObject : null);
    }

    static Button GetButton(UITreeBranch branch)
    {
        return branch != null ? branch.GetComponent<Button>() : null;
    }

    static GameObject GetBranchObject(UITreeBranch branch)
    {
        return branch != null ? branch.gameObject : null;
    }

    static void BindSceneButton(Button button, UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    void EnsureMenuButtonRelay(Button button)
    {
        if (button == null)
            return;

        if (!button.TryGetComponent<WorldMenuButtonRelay>(out var relay))
            relay = button.gameObject.AddComponent<WorldMenuButtonRelay>();

        relay.Bind(this);
    }

    static bool IsDuplicateUiInvoke(ref int lastInvokeFrame, string actionName = null)
    {
        if (lastInvokeFrame == Time.frameCount)
            return true;

        lastInvokeFrame = Time.frameCount;
        return false;
    }
}

public sealed class WorldMenuButtonRelay : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    WorldUIManager manager;

    public void Bind(WorldUIManager target)
    {
        manager = target;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        manager?.Menu();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        manager?.Menu();
    }
}
