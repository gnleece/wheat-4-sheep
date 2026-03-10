using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLocationSelectionObject : MonoBehaviour, IInteractable
{
    private HexEdge _hexEdge;
    private Renderer _renderer;
    
    private Color _hoverColor = Color.yellow;

    public event Action OnRoadLocationSelected;

    public void Initialize(HexEdge hexEdge)
    {
        _hexEdge = hexEdge;
        _renderer = gameObject.GetComponent<Renderer>();
    }

    public void SetHoverColor(Color color)
    {
        _hoverColor = color;
    }

    public void Select()
    {
        OnRoadLocationSelected?.Invoke();
    }

    public void HoverOn()
    {
        if (_hexEdge.IsOccupied)
        {
            return;
        }

        if (_renderer != null)
        {
            _renderer.material.color = _hoverColor;
        }
    }

    public void HoverOff()
    {
        if (_hexEdge.IsOccupied)
        {
            return;
        }

        if (_renderer != null)
        {
            _renderer.material.color = Color.white;
        }
    }
}
