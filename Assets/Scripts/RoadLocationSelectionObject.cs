using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadLocationSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;

    private IBoard board;
    private HexEdge hexEdge;

    public void Initialize(IBoard board, HexEdge hexEdge)
    {
        this.board = board;
        this.hexEdge = hexEdge;

        renderer = gameObject.GetComponent<Renderer>();
    }

    public void Select()
    {
        if (board.TrySelectRoadLocation(hexEdge))
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
            var currentPlayerId = board.GetCurrentPlayerId();
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
