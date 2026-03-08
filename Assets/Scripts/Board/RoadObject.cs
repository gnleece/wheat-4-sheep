using Grid;
using UnityEngine;

public class RoadObject : MonoBehaviour
{
    private new Renderer renderer;

    public void Refresh(Road road)
    {
        var active = road != null;

        gameObject.SetActive(active);

        if (active)
        {
            if (renderer == null)
            {
                renderer = GetComponentInChildren<Renderer>();
            }

            if (renderer != null)
            {
                renderer.material.color = road.Owner.PlayerColor;
            }
        }
    }
}
