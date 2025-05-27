using Grid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingLocationSelectionObject : MonoBehaviour, IInteractable
{
    private new Renderer renderer;

    private IBoardManager boardManager;
    private HexVertex hexVertex;

    public void Initialize(IBoardManager boardManager, HexVertex hexVertex)
    {
        this.boardManager = boardManager;
        this.hexVertex = hexVertex;

        renderer = gameObject.GetComponent<Renderer>();
    }

    public void Select()
    {
        if (boardManager.TrySelectSettlementLocation(hexVertex))
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
