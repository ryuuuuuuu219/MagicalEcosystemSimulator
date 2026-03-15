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
    MenuNode genomeViewerNode;
    MenuNode genomeInjectorNode;

    void EnsureMenuTree()
    {
        if (menuRoot != null)
            return;

        menuRoot = new MenuNode { name = "MenuRoot" };
        objectListNode = CreateMenuNode("ObjectList", GetNodeObject(TabUIlist, 0), menuRoot);
        generationNode = CreateMenuNode("Generation", GetNodeObject(TabUIlist, 1), menuRoot);
        advanceGenerationNode = CreateMenuNode("AdvanceGeneration", GetNodeObject(GencontrollerUIlist, 0), generationNode);
        genomeViewerNode = CreateMenuNode("GenomeViewer", GetNodeObject(GencontrollerUIlist, 1), generationNode);
        genomeInjectorNode = CreateMenuNode("GenomeInjector", GetNodeObject(GencontrollerUIlist, 2), generationNode);
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

        node.visible = false;

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

    void ActivateNode(MenuNode node)
    {
        if (node == null || node.parent == null)
            return;

        foreach (var sibling in node.parent.children)
        {
            if (sibling == node)
                continue;

            HideBranch(sibling);
        }

        node.visible = true;

        if (node.uiObject != null)
            node.uiObject.SetActive(true);
    }

    void ShowDirectChildren(MenuNode node)
    {
        if (node == null)
            return;

        foreach (var child in node.children)
        {
            child.visible = true;
            if (child.uiObject != null)
                child.uiObject.SetActive(true);
        }
    }

    void ShowMenuRootButtons()
    {
        EnsureMenuTree();
        HideBranch(objectListNode);
        HideBranch(generationNode);

        objectListNode.visible = true;
        if (objectListNode.uiObject != null)
            objectListNode.uiObject.SetActive(true);

        generationNode.visible = true;
        if (generationNode.uiObject != null)
            generationNode.uiObject.SetActive(true);
    }

    void HideAllMenuBranches()
    {
        EnsureMenuTree();
        HideBranch(objectListNode);
        HideBranch(generationNode);
    }

    void OpenObjectListBranch()
    {
        EnsureMenuTree();
        ShowMenuRootButtons();
        HideChildren(generationNode);
        objectListNode.visible = true;
        if (objectListNode.uiObject != null)
            objectListNode.uiObject.SetActive(true);

        isObjectListVisible = true;
        RefreshObjectSources();
        ClearObjectList();
        DisplayObjectList();
    }

    void OpenGenerationBranch()
    {
        EnsureMenuTree();
        ShowMenuRootButtons();
        HideChildren(objectListNode);
        generationNode.visible = true;
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

    void OpenAdvanceGenerationBranch()
    {
        EnsureMenuTree();
        OpenGenerationBranch();
        ActivateNode(advanceGenerationNode);
        if (generationController != null)
        {
            generationController.HideGenomePanels();
            generationController.onclickbutton2_1();
        }
    }

    void OpenGenomeViewerBranch()
    {
        EnsureMenuTree();
        OpenGenerationBranch();
        ActivateNode(genomeViewerNode);
        if (generationController != null)
        {
            generationController.SetGenomeViewerVisible(true);
            generationController.SetGenomeInjectorVisible(false);
        }
    }

    void OpenGenomeInjectorBranch()
    {
        EnsureMenuTree();
        OpenGenerationBranch();
        ActivateNode(genomeInjectorNode);
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
}
