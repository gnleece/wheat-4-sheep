using Grid;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTileSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;
    private HexTile hexTile;
    private Color hoverColor = Color.yellow;

    public event Action OnHexTileSelected;

    public void Initialize(HexTile hexTile)
    {
        this.hexTile = hexTile;
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }

    public void Select()
    {
        OnHexTileSelected?.Invoke();
    }

    public void HoverOn()
    {
        if (renderer != null)
        {
            renderer.material.color = hoverColor;
        }
    }

    public void HoverOff()
    {
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }
}
