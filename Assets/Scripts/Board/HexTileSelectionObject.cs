using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileSelectionObject : MonoBehaviour, IInteractable
{
    public HexTile Tile { get; private set; }
    
    private Renderer _renderer;
    
    private Color _hoverColor = Color.yellow;

    public event Action OnHexTileSelected;

    public void Initialize(HexTile hexTile)
    {
        Tile = hexTile;
        _renderer = gameObject.GetComponent<Renderer>();
    }

    public void SetHoverColor(Color color)
    {
        _hoverColor = color;
    }

    public void Select()
    {
        OnHexTileSelected?.Invoke();
    }

    public void HoverOn()
    {
        if (_renderer != null)
        {
            _renderer.material.color = _hoverColor;
        }
    }

    public void HoverOff()
    {
        if (_renderer != null)
        {
            _renderer.material.color = Color.white;
        }
    }
}
