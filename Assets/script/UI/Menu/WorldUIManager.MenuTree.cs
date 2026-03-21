using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MenuNode
{
    public string name;
    public GameObject uiObject;
    public MenuNode parent;
    public List<MenuNode> children = new();
    public bool visible;
}

public partial class WorldUIManager
{
    MenuNode menuRoot;
    MenuNode objectListNode;
    MenuNode generationNode;
    MenuNode advanceGenerationNode;

    void EnsureMenuTree()
    {
        if (menuRoot != null)
            return;

        menuRoot = new MenuNode { name = "MenuRoot" };
        objectListNode = CreateMenuNode("ObjectList", GetNodeObject(TabUIlist, 0), menuRoot);
        generationNode = CreateMenuNode("Generation", GetNodeObject(TabUIlist, 1), menuRoot);
        advanceGenerationNode = CreateMenuNode("AdvanceGeneration", GetAdvanceGenerationButton()?.gameObject, generationNode);
        CreateMenuNode("GenomeViewer", GetGenomeViewerButton()?.gameObject, generationNode);
        CreateMenuNode("GenomeInjector", GetGenomeInjectorButton()?.gameObject, generationNode);
    }

    MenuNode CreateMenuNode(string name, GameObject uiObject, MenuNode parent)
    {
        MenuNode node = new MenuNode
        {
            name = name,
            uiObject = uiObject,
            parent = parent
        };

        if (parent != null)
            parent.children.Add(node);

        return node;
    }

    GameObject GetNodeObject(List<GameObject> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    void HideBranch(MenuNode node)
    {
        if (node == null)
            return;

        if (node.uiObject != null)
            node.uiObject.SetActive(false);

        foreach (var child in node.children)
            HideBranch(child);
    }

    void HideChildren(MenuNode node)
    {
        if (node == null)
            return;

        foreach (var child in node.children)
            HideBranch(child);
    }

    void ShowDirectChildren(MenuNode node)
    {
        if (node == null)
            return;

        foreach (var child in node.children)
        {
            if (child.uiObject != null)
                child.uiObject.SetActive(true);
        }
    }

    void ShowMenuRootButtons()
    {
        EnsureMenuTree();
        HideBranch(objectListNode);
        HideBranch(generationNode);

        if (objectListNode.uiObject != null)
            objectListNode.uiObject.SetActive(true);

        if (generationNode.uiObject != null)
            generationNode.uiObject.SetActive(true);
    }

    void HideAllMenuBranches()
    {
        EnsureMenuTree();
        HideBranch(objectListNode);
        HideBranch(generationNode);
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
        if (objectListNode.uiObject != null)
            objectListNode.uiObject.SetActive(true);

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
        if (generationNode.uiObject != null)
            generationNode.uiObject.SetActive(true);
        ShowDirectChildren(generationNode);

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
        if (advanceGenerationNode.uiObject != null)
            advanceGenerationNode.uiObject.SetActive(true);
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
        ShowDirectChildren(generationNode);
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
        ShowDirectChildren(generationNode);
        if (generationController != null)
        {
            generationController.SetGenomeViewerVisible(false);
            generationController.SetGenomeInjectorVisible(true);
        }
    }

    void HideGenerationBranchContent()
    {
        foreach (GameObject go in GencontrollerUIlist)
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
        HideStatusUI();
        ClearObjectList();
        ClearStateview();
        if (objectListNode.uiObject != null)
            objectListNode.uiObject.SetActive(true);
    }

    /// <summary>
    /// Generation 配下の子パネルを閉じ、親ボタンのみ残します。
    /// </summary>
    /// <seealso cref="OpenGenerationBranch"/>
    void CloseGenerationBranch()
    {
        EnsureMenuTree();
        HideChildren(generationNode);
        HideGenerationBranchContent();
        if (generationNode.uiObject != null)
            generationNode.uiObject.SetActive(true);
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
        ShowDirectChildren(generationNode);
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
        ShowDirectChildren(generationNode);
    }

    bool AreGenerationBranchControlsVisible()
    {
        EnsureMenuTree();
        foreach (var child in generationNode.children)
        {
            if (child.uiObject != null && child.uiObject.activeSelf)
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
