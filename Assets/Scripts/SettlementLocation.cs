using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettlementLocation : MonoBehaviour, IInteractable
{
    private new Renderer renderer;

    private IBoard board;
    private HexVertex hexVertex;

    public void Initialize(IBoard board, HexVertex hexVertex)
    {
        this.board = board;
        this.hexVertex = hexVertex;

        renderer = gameObject.GetComponent<Renderer>();
    }

    public void Select()
    {
        if (board.SettlementLocationSelected(hexVertex))
        {
            if (renderer != null)
            {
                PlayerColorManager.ApplyColorToRenderer(renderer, hexVertex.Owner.PlayerId);
            }
        }
    }

    public void HoverOn()
    {
        if (hexVertex.IsOccupied)
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
