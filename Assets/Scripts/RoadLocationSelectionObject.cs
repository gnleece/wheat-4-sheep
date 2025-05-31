using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLocationSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;
    private HexEdge hexEdge;

    public event Action OnRoadLocationSelected;

    public void Initialize(HexEdge hexEdge)
    {
        this.hexEdge = hexEdge;
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void Select()
    {
        OnRoadLocationSelected?.Invoke();
    }

    public void HoverOn()
    {
        if (hexEdge.IsOccupied)
        {
            return;
        }

        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }

    public void HoverOff()
    {
        if (hexEdge.IsOccupied)
        {
            return;
        }

        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
}
