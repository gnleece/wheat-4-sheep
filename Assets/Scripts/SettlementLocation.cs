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
                renderer.material.color = Color.green;
            }
        }
    }

    public void HoverOn()
    {
        if (renderer != null)
        {
            renderer.material.color = Color.red;
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
