using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLocationSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;

    private IBoardManager boardManager;
    private HexEdge hexEdge;

    public void Initialize(IBoardManager boardManager, HexEdge hexEdge)
    {
        this.boardManager = boardManager;
        this.hexEdge = hexEdge;

        renderer = gameObject.GetComponent<Renderer>();
    }

    public void Select()
    {
        if (boardManager.TrySelectRoadLocation(hexEdge))
        {
            if (renderer != null)
            {
                PlayerColorManager.ApplyColorToRenderer(renderer, hexEdge.Road.Owner.PlayerId);
            }
        }
    }

    public void HoverOn()
    {
        if (hexEdge.IsOccupied)
        {
            return;
        }

        if (renderer != null)
        {
            var currentPlayerId = boardManager.GetCurrentPlayerId();
            if (currentPlayerId.HasValue)
            {
                PlayerColorManager.ApplyColorToRenderer(renderer, currentPlayerId.Value);
            }
            else
            {
                renderer.material.color = Color.red;
            }
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
