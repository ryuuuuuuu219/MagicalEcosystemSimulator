using System;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class WorldUIManager
{
    void UpdateCameraFollow()
    {
        if (currentTarget == null)
        {
            if (text_f != null)
                text_f.text = string.Empty;
            return;
        }

        Vector3 currentPos = mainCamera.transform.rotation * Vector3.forward * 20f;
        mainCamera.transform.position = currentTarget.transform.position - currentPos;
        UpdateFollowText();
    }

    void HandleWorldClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Objectpic();
        UpdateFollowText();
    }

    void Objectpic()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        RaycastHit[] hits = Physics.RaycastAll(mainCamera.ScreenPointToRay(Input.mousePosition), 1000f, selectableLayer);
        if (hits.Length == 0)
            return;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        SetTarget(PickTarget(hits));

        if (!isWorldMenuVisible)
        {
            isWorldMenuVisible = true;
            ShowMenuRootButtons();
        }
    }

    GameObject PickTarget(RaycastHit[] hits)
    {
        GameObject target = hits[0].collider.gameObject;
        if (target != currentTarget || !IsCreature(target))
            return target;

        for (int i = 1; i < hits.Length; i++)
        {
            GameObject alternate = hits[i].collider.gameObject;
            if (alternate != currentTarget && IsCreature(alternate))
                return alternate;
        }

        return target;
    }

    static bool IsCreature(GameObject obj)
    {
        return obj != null &&
               (obj.TryGetComponent<herbivoreBehaviour>(out _) ||
                obj.TryGetComponent<predatorBehaviour>(out _));
    }

    void UpdateFollowText()
    {
        if (text_f == null)
            return;

        if (currentTarget == null)
        {
            text_f.text = string.Empty;
            return;
        }

        if (isStatusVisible)
            return;

        text_f.text = BuildFollowText();
    }

    string BuildFollowText()
    {
        string statusText = "follow:" + currentTarget.name;

        if (currentTarget.TryGetComponent<Resource>(out var resource))
        {
            statusText += "\nmana:" + resource.mana.ToString("F1");
            statusText += "\nmaxMana:" + resource.maxMana.ToString("F1");
            statusText += "\ncategory:" + resource.resourceCategory;
        }

        if (currentTarget.TryGetComponent<herbivoreBehaviour>(out var herbivore))
        {
            statusText += "\nhealth:" + herbivore.health.ToString("F1");
            statusText += "\nmana:" + herbivore.mana.ToString("F1");
            statusText += "\ndead:" + herbivore.IsDead;
        }
        else if (currentTarget.TryGetComponent<predatorBehaviour>(out var predator))
        {
            statusText += "\nhealth:" + predator.health.ToString("F1");
            statusText += "\nmana:" + predator.mana.ToString("F1");
            statusText += "\ndead:" + predator.IsDead;
        }

        return statusText;
    }
}
