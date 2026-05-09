using UnityEngine;

public partial class WorldUIManager
{
    void InitializeMenuBranchVisibility()
    {
        isWorldMenuVisible = false;
        isObjectListVisible = false;
        isStatusVisible = false;

        UITreeBranch.HideAllExcept(menuRootBranch);
        SetPendingSettingsVisible(false);
        HideStatusUI();
        ClearStateview();
    }

    void ShowMenuRootButtons()
    {
        PushBranch(menuRootBranch);
        SetPendingSettingsVisible(false);
    }

    void HideAllMenuBranches()
    {
        UITreeBranch.HideAllExcept(menuRootBranch);
        SetPendingSettingsVisible(false);
    }

    /// <summary>
    /// オブジェクト一覧ブランチを開き、他ブランチを閉じたうえで一覧を再構築します。
    /// </summary>
    /// <seealso cref="ToggleObjectListBranch"/>
    /// <seealso cref="CloseObjectListBranch"/>
    void OpenObjectListBranch()
    {
        ResetObjectListState(false);
        HideGenerationBranchContent();
        SetPendingSettingsVisible(false);
        PushBranch(objectListBranch);

        isObjectListVisible = true;
        RefreshObjectSources();
        ClearObjectList();
        DisplayObjectList();
    }

    /// <summary>
    /// Generation ブランチを開き、子メニューを表示します。
    /// </summary>
    /// <seealso cref="ToggleGenerationBranch"/>
    /// <seealso cref="CloseGenerationBranch"/>
    void OpenGenerationBranch()
    {
        PrepareGenerationBranch();
        PushBranch(generationBranch);
    }

    void OpenPropertiesBranch()
    {
        ResetObjectListState(true);
        HideGenerationBranchContent();
        HideExperimentBranchContent();
        PushBranch(propertiesBranch);
        OpenAllPendingSettingsPanel();
    }

    void OpenFieldViewingBranch()
    {
        ResetObjectListState(true);
        HideGenerationBranchContent();
        SetPendingSettingsVisible(false);
        SetExperimentBranchContentVisible(fieldViewingDropdownBranch, true);
        SetExperimentBranchContentVisible(disturbanceElementBranch, false);
        PushBranch(fieldViewingTab);
    }

    void OpenDisturbanceBranch()
    {
        ResetObjectListState(true);
        HideGenerationBranchContent();
        SetPendingSettingsVisible(false);
        SetExperimentBranchContentVisible(fieldViewingDropdownBranch, false);
        SetExperimentBranchContentVisible(disturbanceElementBranch, true);
        PushBranch(disturbanceTab);
    }

    /// <summary>
    /// AdvanceGeneration ブランチを開き、世代更新処理を呼び出します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void OpenAdvanceGenerationBranch()
    {
        PrepareGenerationBranch();
        PushBranch(advanceGenerationBranch);
        if (generationController != null)
            generationController.HideGenomePanels();
        OpenGenerationSettingsPanel();
    }

    /// <summary>
    /// Genome Viewer ブランチを開きます。
    /// </summary>
    /// <seealso cref="ToggleGenomeViewerBranch"/>
    /// <seealso cref="CloseGenomeViewerBranch"/>
    void OpenGenomeViewerBranch()
    {
        PrepareGenerationBranch();
        PushBranch(genomeViewerBranch);
        if (generationController != null)
        {
            generationController.SetGenomeViewerVisible(true);
            generationController.SetGenomeInjectorVisible(false);
        }
    }

    /// <summary>
    /// Genome Injector ブランチを開きます。
    /// </summary>
    /// <seealso cref="ToggleGenomeInjectorBranch"/>
    /// <seealso cref="CloseGenomeInjectorBranch"/>
    void OpenGenomeInjectorBranch()
    {
        PrepareGenerationBranch();
        PushBranch(genomeInjectorBranch);
        if (generationController != null)
        {
            generationController.SetGenomeViewerVisible(false);
            generationController.SetGenomeInjectorVisible(true);
        }
    }

    void HideGenerationBranchContent()
    {
        if (generationController != null)
            generationController.HideGenomePanels();
    }

    void HideExperimentBranchContent()
    {
        SetExperimentBranchContentVisible(fieldViewingDropdownBranch, false);
        SetExperimentBranchContentVisible(disturbanceElementBranch, false);
    }

    static void SetExperimentBranchContentVisible(UITreeBranch branch, bool visible)
    {
        if (branch != null)
            branch.gameObject.SetActive(visible);
    }

    /// <summary>
    /// オブジェクト一覧ブランチの表示をトグルします。
    /// </summary>
    /// <seealso cref="OpenObjectListBranch"/>
    /// <seealso cref="CloseObjectListBranch"/>
    void ToggleObjectListBranch()
    {
        if (isObjectListVisible)
        {
            CloseObjectListBranch();
            return;
        }

        OpenObjectListBranch();
    }

    /// <summary>
    /// Generation ブランチの表示をトグルします。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    /// <seealso cref="CloseGenerationBranch"/>
    void ToggleGenerationBranch()
    {
        if (AreGenerationBranchControlsVisible())
        {
            CloseGenerationBranch();
            return;
        }

        OpenGenerationBranch();
    }

    /// <summary>
    /// Genome Viewer ブランチの表示をトグルします。
    /// </summary>
    /// <seealso cref="OpenGenomeViewerBranch"/>
    /// <seealso cref="CloseGenomeViewerBranch"/>
    void ToggleGenomeViewerBranch()
    {
        if (IsGenomeViewerBranchOpen())
        {
            CloseGenomeViewerBranch();
            return;
        }

        OpenGenomeViewerBranch();
    }

    /// <summary>
    /// Genome Injector ブランチの表示をトグルします。
    /// </summary>
    /// <seealso cref="OpenGenomeInjectorBranch"/>
    /// <seealso cref="CloseGenomeInjectorBranch"/>
    void ToggleGenomeInjectorBranch()
    {
        if (IsGenomeInjectorBranchOpen())
        {
            CloseGenomeInjectorBranch();
            return;
        }

        OpenGenomeInjectorBranch();
    }

    void TogglePropertiesBranch()
    {
        if (IsPropertiesBranchOpen())
        {
            ClosePropertiesBranch();
            return;
        }

        OpenPropertiesBranch();
    }

    void ToggleFieldViewingBranch()
    {
        if (IsFieldViewingBranchOpen())
        {
            CloseFieldViewingBranch();
            return;
        }

        OpenFieldViewingBranch();
    }

    void ToggleDisturbanceBranch()
    {
        if (IsDisturbanceBranchOpen())
        {
            CloseDisturbanceBranch();
            return;
        }

        OpenDisturbanceBranch();
    }

    /// <summary>
    /// オブジェクト一覧ブランチと状態表示 UI を閉じます。
    /// </summary>
    /// <seealso cref="OpenObjectListBranch"/>
    void CloseObjectListBranch()
    {
        ResetObjectListState(true);
        PushBranch(menuRootBranch);
    }

    /// <summary>
    /// Generation 配下の子パネルを閉じ、親ボタンのみ残します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void CloseGenerationBranch()
    {
        HideGenerationBranchContent();
        PushBranch(menuRootBranch);
    }

    void ClosePropertiesBranch()
    {
        pendingSettingsReturnsToGenerationBranch = false;
        pendingSettingsView = PendingSettingsView.All;
        SetPendingSettingsVisible(false);
        PushBranch(menuRootBranch);
    }

    void CloseFieldViewingBranch()
    {
        SetExperimentBranchContentVisible(fieldViewingDropdownBranch, false);
        PushBranch(menuRootBranch);
    }

    void CloseDisturbanceBranch()
    {
        SetExperimentBranchContentVisible(disturbanceElementBranch, false);
        PushBranch(menuRootBranch);
    }

    /// <summary>
    /// Genome Viewer パネルを閉じます。
    /// </summary>
    /// <seealso cref="OpenGenomeViewerBranch"/>
    void CloseGenomeViewerBranch()
    {
        if (generationController != null)
            generationController.SetGenomeViewerVisible(false);
        PushBranch(generationBranch);
    }

    /// <summary>
    /// Genome Injector パネルを閉じます。
    /// </summary>
    /// <seealso cref="OpenGenomeInjectorBranch"/>
    void CloseGenomeInjectorBranch()
    {
        if (generationController != null)
            generationController.SetGenomeInjectorVisible(false);
        PushBranch(generationBranch);
    }

    bool AreGenerationBranchControlsVisible()
    {
        if (generationBranch == null)
            return false;

        return generationBranch.HasActiveChild();
    }

    bool IsGenomeViewerBranchOpen()
    {
        return generationController != null &&
               generationController.herbivoreGenomeViewerRoot != null &&
               generationController.herbivoreGenomeViewerRoot.activeSelf;
    }

    bool IsGenomeInjectorBranchOpen()
    {
        return generationController != null &&
               generationController.herbivoreGenomeInjectorRoot != null &&
               generationController.herbivoreGenomeInjectorRoot.activeSelf;
    }

    bool IsPropertiesBranchOpen()
    {
        return propertiesBranch != null &&
               propertiesBranch.gameObject.activeSelf &&
               pendingSettingsPanelRoot != null &&
               pendingSettingsPanelRoot.activeSelf;
    }

    bool IsFieldViewingBranchOpen()
    {
        return fieldViewingTab != null &&
               fieldViewingTab.gameObject.activeSelf &&
               fieldViewingDropdownBranch != null &&
               fieldViewingDropdownBranch.gameObject.activeSelf;
    }

    bool IsDisturbanceBranchOpen()
    {
        return disturbanceTab != null &&
               disturbanceTab.gameObject.activeSelf &&
               disturbanceElementBranch != null &&
               disturbanceElementBranch.gameObject.activeSelf;
    }

    static void PushBranch(UITreeBranch branch)
    {
        if (branch == null)
            return;

        branch.Push();
    }

    void ResetObjectListState(bool clearList)
    {
        isObjectListVisible = false;
        isStatusVisible = false;

        HideStatusUI();
        ClearStateview();
        HideExperimentBranchContent();
        if (clearList)
            ClearObjectList();
    }

    void PrepareGenerationBranch()
    {
        ResetObjectListState(true);
        SetPendingSettingsVisible(false);
        isObjectListVisible = false;
        HideExperimentBranchContent();
        HideGenerationBranchContent();
    }
}
