using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class UITreeBranch : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] GameObject parent;
    [SerializeField] List<GameObject> children = new();
    [SerializeField] int[] eachID = new int[0];
    [SerializeField] bool pushOnPointerEvent = false;

    public bool isOpen;

    public IReadOnlyList<GameObject> Children => children;

    static readonly List<UITreeBranch> branches = new();

    void OnEnable()
    {
        if (!branches.Contains(this))
            branches.Add(this);
    }

    void OnDestroy()
    {
        branches.Remove(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!pushOnPointerEvent)
            return;

        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        Push();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!pushOnPointerEvent)
            return;

        Push();
    }

    public void Push()
    {
        if (!HasValidID())
            return;

        PushID(eachID);
    }

    public static void PushID(IReadOnlyList<int> pushedID)
    {
        if (pushedID == null)
            return;

        RefreshBranches();
        for (int i = branches.Count - 1; i >= 0; i--)
        {
            if (branches[i] == null)
            {
                branches.RemoveAt(i);
                continue;
            }

            branches[i].ApplyPushedID(pushedID);
        }
    }

    public static void HideAllExcept(UITreeBranch visibleBranch)
    {
        RefreshBranches();
        foreach (UITreeBranch branch in branches)
        {
            if (branch == null)
                continue;

            bool visible = branch == visibleBranch;
            branch.isOpen = false;
            branch.gameObject.SetActive(visible);
        }
    }

    static void RefreshBranches()
    {
        branches.Clear();
        branches.AddRange(FindObjectsByType<UITreeBranch>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    public bool HasActiveChild()
    {
        foreach (GameObject child in children)
        {
            if (child != null && child.activeSelf)
                return true;
        }

        return false;
    }

    void ApplyPushedID(IReadOnlyList<int> pushedID)
    {
        bool parentPathMatches = IsParentPathMatched(pushedID);
        bool selfPathMatches = IsPrefixOfPushedID(pushedID);

        isOpen = selfPathMatches;
        gameObject.SetActive(parentPathMatches);
    }

    bool IsParentPathMatched(IReadOnlyList<int> pushedID)
    {
        if (eachID == null || eachID.Length == 0)
            return IsRootBranch();

        if (pushedID.Count < eachID.Length - 1)
            return false;

        for (int i = 0; i < eachID.Length - 1; i++)
        {
            if (eachID[i] != pushedID[i])
                return false;
        }

        return true;
    }

    bool IsPrefixOfPushedID(IReadOnlyList<int> pushedID)
    {
        if (eachID == null || eachID.Length == 0)
            return IsRootBranch();

        if (pushedID.Count < eachID.Length)
            return false;

        for (int i = 0; i < eachID.Length; i++)
        {
            if (eachID[i] != pushedID[i])
                return false;
        }

        return true;
    }

    bool HasValidID()
    {
        return IsRootBranch() || (eachID != null && eachID.Length > 0);
    }

    bool IsRootBranch()
    {
        return parent == null;
    }

}
