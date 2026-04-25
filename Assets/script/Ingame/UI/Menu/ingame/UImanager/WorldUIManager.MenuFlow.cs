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
        SetBranchOpen(menuRootBranch, false);
        SetBranchDescendantsVisible(objectListBranch, false);
        SetBranchDescendantsVisible(generationBranch, false);
        SetBranchDescendantsVisible(propertiesBranch, false);
        SetPendingSettingsVisible(false);
        SetBranchVisible(objectListBranch, false);
        SetBranchVisible(generationBranch, false);
        SetBranchVisible(propertiesBranch, false);
    }

    /// <summary>
    /// オブジェクト一覧ブランチを開き、他ブランチを閉じたうえで一覧を再構築します。
    /// </summary>
    /// <seealso cref="ToggleObjectListBranch"/>
    /// <seealso cref="CloseObjectListBranch"/>
    void OpenObjectListBranch()
    {
        ShowMenuRootButtons();
        CloseGenerationBranch();
        ClosePropertiesBranch();
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
        ShowMenuRootButtons();
        CloseObjectListBranch();
        ClosePropertiesBranch();
        PushBranch(generationBranch);

        isObjectListVisible = false;
        ClearObjectList();
        HideStatusUI();
        ClearStateview();
        if (generationController != null)
            generationController.HideGenomePanels();
    }

    void OpenPropertiesBranch()
    {
        ShowMenuRootButtons();
        CloseObjectListBranch();
        CloseGenerationBranch();
        PushBranch(propertiesBranch);
        SetPendingSettingsVisible(true);
    }

    /// <summary>
    /// AdvanceGeneration ブランチを開き、世代更新処理を呼び出します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void OpenAdvanceGenerationBranch()
    {
        OpenGenerationBranch();
        PushBranch(advanceGenerationBranch);
        if (generationController != null)
        {
            generationController.HideGenomePanels();
            generationController.onclickbutton2_1();
        }
    }

    /// <summary>
    /// Genome Viewer ブランチを開きます。
    /// </summary>
    /// <seealso cref="ToggleGenomeViewerBranch"/>
    /// <seealso cref="CloseGenomeViewerBranch"/>
    void OpenGenomeViewerBranch()
    {
        OpenGenerationBranch();
        PushBranch(generationBranch);
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
        OpenGenerationBranch();
        PushBranch(generationBranch);
        if (generationController != null)
        {
            generationController.SetGenomeViewerVisible(false);
            generationController.SetGenomeInjectorVisible(true);
        }
    }

    void HideGenerationBranchContent()
    {
        foreach (GameObject go in EnumerateGenerationButtons())
        {
            if (go == null) continue;
            go.SetActive(false);
        }

        if (generationController != null)
            generationController.HideGenomePanels();
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

    /// <summary>
    /// オブジェクト一覧ブランチと状態表示 UI を閉じます。
    /// </summary>
    /// <seealso cref="OpenObjectListBranch"/>
    void CloseObjectListBranch()
    {
        isObjectListVisible = false;
        isStatusVisible = false;
        SetBranchOpen(objectListBranch, false);
        SetBranchDescendantsVisible(objectListBranch, false);
        HideStatusUI();
        ClearObjectList();
        ClearStateview();
        SetBranchVisible(objectListBranch, true);
    }

    /// <summary>
    /// Generation 配下の子パネルを閉じ、親ボタンのみ残します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void CloseGenerationBranch()
    {
        SetBranchOpen(generationBranch, false);
        SetBranchDescendantsVisible(generationBranch, false);
        HideGenerationBranchContent();
        SetBranchVisible(generationBranch, true);
    }

    void ClosePropertiesBranch()
    {
        SetBranchOpen(propertiesBranch, false);
        SetBranchDescendantsVisible(propertiesBranch, false);
        SetPendingSettingsVisible(false);
        SetBranchVisible(propertiesBranch, true);
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

    static void PushBranch(UITreeBranch branch)
    {
        if (branch == null)
            return;

        branch.Push();
    }

    static void SetBranchOpen(UITreeBranch branch, bool open)
    {
        if (branch == null)
            return;

        branch.SetOpen(open);
    }

    static void SetBranchVisible(UITreeBranch branch, bool visible)
    {
        if (branch == null)
            return;

        branch.SetVisible(visible);
    }

    static void SetBranchDescendantsVisible(UITreeBranch branch, bool visible)
    {
        if (branch == null)
            return;

        branch.SetDescendantsVisible(visible);
    }
}
