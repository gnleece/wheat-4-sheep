using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLocationSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;
    private HexVertex hexVertex;
    private Color hoverColor = Color.yellow;

    public event Action OnBuildingLocationSelected;

    public void Initialize(HexVertex hexVertex)
    {
        this.hexVertex = hexVertex;
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }

    public void Select()
    {
        OnBuildingLocationSelected?.Invoke();
    }

    public void HoverOn()
    {
        if (hexVertex.IsOccupied)
        {
            return;
        }

        if (renderer != null)
        {
            renderer.material.color = hoverColor;
        }
    }

    public void HoverOff()
    {
        if (hexVertex.IsOccupied)
        {
            return;
        }

        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
}
