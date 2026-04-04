using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UITreeBranch
{
    public GameObject entity;
    public UITreeBranch parent;
    public List<UITreeBranch> children = new();
    public bool isOpen;
}

public partial class WorldUIManager
{
    UITreeBranch menuRootBranch;
    UITreeBranch objectListBranch;
    UITreeBranch generationBranch;
    UITreeBranch advanceGenerationBranch;

    void EnsureMenuTree()
    {
        if (menuRootBranch != null)
            return;

        menuRootBranch = new UITreeBranch();
        objectListBranch = CreateBranch(ObjectList_tab_00, menuRootBranch);
        generationBranch = CreateBranch(GenController_tab_01, menuRootBranch);
        advanceGenerationBranch = CreateBranch(GetAdvanceGenerationButton()?.gameObject, generationBranch);
        CreateBranch(GetGenomeViewerButton()?.gameObject, generationBranch);
        CreateBranch(GetGenomeInjectorButton()?.gameObject, generationBranch);
    }

    UITreeBranch CreateBranch(GameObject entity, UITreeBranch parent)
    {
        UITreeBranch branch = new UITreeBranch
        {
            entity = entity,
            parent = parent
        };

        if (parent != null)
            parent.children.Add(branch);

        return branch;
    }

    void SetBranchOpen(UITreeBranch branch, bool open)
    {
        if (branch == null)
            return;

        branch.isOpen = open;
    }

    void SetDirectChildrenActive(UITreeBranch branch, bool active)
    {
        if (branch == null)
            return;

        foreach (var child in branch.children)
        {
            SetBranchOpen(child, active);
            if (child.entity != null)
                child.entity.SetActive(active);
        }
    }

    void SetDescendantsActive(UITreeBranch branch, bool active)
    {
        if (branch == null)
            return;

        foreach (var child in branch.children)
        {
            SetBranchOpen(child, active);
            if (child.entity != null)
                child.entity.SetActive(active);

            SetDescendantsActive(child, active);
        }
    }

    void CloseDescendants(UITreeBranch branch)
    {
        SetDescendantsActive(branch, false);
    }

    void ShowMenuRootButtons()
    {
        EnsureMenuTree();
        CloseDescendants(objectListBranch);
        CloseDescendants(generationBranch);

        if (objectListBranch.entity != null)
            objectListBranch.entity.SetActive(true);

        if (generationBranch.entity != null)
            generationBranch.entity.SetActive(true);
    }

    void HideAllMenuBranches()
    {
        EnsureMenuTree();
        CloseDescendants(objectListBranch);
        CloseDescendants(generationBranch);
        if (objectListBranch.entity != null)
            objectListBranch.entity.SetActive(false);
        if (generationBranch.entity != null)
            generationBranch.entity.SetActive(false);
    }

    /// <summary>
    /// オブジェクト一覧ブランチを開き、他ブランチを閉じたうえで一覧を再構築します。
    /// </summary>
    /// <seealso cref="ToggleObjectListBranch"/>
    /// <seealso cref="CloseObjectListBranch"/>
    void OpenObjectListBranch()
    {
        EnsureMenuTree();
        ShowMenuRootButtons();
        CloseGenerationBranch();
        SetBranchOpen(objectListBranch, true);
        if (objectListBranch.entity != null)
            objectListBranch.entity.SetActive(true);

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
        EnsureMenuTree();
        ShowMenuRootButtons();
        CloseObjectListBranch();
        SetBranchOpen(generationBranch, true);
        if (generationBranch.entity != null)
            generationBranch.entity.SetActive(true);
        SetDirectChildrenActive(generationBranch, true);

        isObjectListVisible = false;
        ClearObjectList();
        HideStatusUI();
        ClearStateview();
        if (generationController != null)
            generationController.HideGenomePanels();
    }

    /// <summary>
    /// AdvanceGeneration ブランチを開き、世代更新処理を呼び出します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void OpenAdvanceGenerationBranch()
    {
        EnsureMenuTree();
        OpenGenerationBranch();
        SetBranchOpen(advanceGenerationBranch, true);
        if (advanceGenerationBranch.entity != null)
            advanceGenerationBranch.entity.SetActive(true);
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
        EnsureMenuTree();
        OpenGenerationBranch();
        SetDirectChildrenActive(generationBranch, true);
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
        EnsureMenuTree();
        OpenGenerationBranch();
        SetDirectChildrenActive(generationBranch, true);
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

    /// <summary>
    /// オブジェクト一覧ブランチと状態表示 UI を閉じます。
    /// </summary>
    /// <seealso cref="OpenObjectListBranch"/>
    void CloseObjectListBranch()
    {
        EnsureMenuTree();
        isObjectListVisible = false;
        isStatusVisible = false;
        SetBranchOpen(objectListBranch, false);
        CloseDescendants(objectListBranch);
        HideStatusUI();
        ClearObjectList();
        ClearStateview();
        if (objectListBranch.entity != null)
            objectListBranch.entity.SetActive(true);
    }

    /// <summary>
    /// Generation 配下の子パネルを閉じ、親ボタンのみ残します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void CloseGenerationBranch()
    {
        EnsureMenuTree();
        SetBranchOpen(generationBranch, false);
        CloseDescendants(generationBranch);
        HideGenerationBranchContent();
        if (generationBranch.entity != null)
            generationBranch.entity.SetActive(true);
    }

    /// <summary>
    /// Genome Viewer パネルを閉じます。
    /// </summary>
    /// <seealso cref="OpenGenomeViewerBranch"/>
    void CloseGenomeViewerBranch()
    {
        EnsureMenuTree();
        if (generationController != null)
            generationController.SetGenomeViewerVisible(false);
        SetDirectChildrenActive(generationBranch, true);
    }

    /// <summary>
    /// Genome Injector パネルを閉じます。
    /// </summary>
    /// <seealso cref="OpenGenomeInjectorBranch"/>
    void CloseGenomeInjectorBranch()
    {
        EnsureMenuTree();
        if (generationController != null)
            generationController.SetGenomeInjectorVisible(false);
        SetDirectChildrenActive(generationBranch, true);
    }

    bool AreGenerationBranchControlsVisible()
    {
        EnsureMenuTree();
        foreach (var child in generationBranch.children)
        {
            if (child.entity != null && child.entity.activeSelf)
                return true;
        }

        return false;
    }

    bool IsGenomeViewerBranchOpen()
    {
        EnsureMenuTree();
        return generationController != null &&
               generationController.herbivoreGenomeViewerRoot != null &&
               generationController.herbivoreGenomeViewerRoot.activeSelf;
    }

    bool IsGenomeInjectorBranchOpen()
    {
        EnsureMenuTree();
        return generationController != null &&
               generationController.herbivoreGenomeInjectorRoot != null &&
               generationController.herbivoreGenomeInjectorRoot.activeSelf;
    }
}
