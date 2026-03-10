using Grid;
using UnityEngine;

public class RoadObject : MonoBehaviour
{
    private Renderer _renderer;

    public void Refresh(Road road)
    {
        var active = road != null;

        gameObject.SetActive(active);

        if (active)
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            if (_renderer != null)
            {
                _renderer.material.color = road.Owner.PlayerColor;
            }
        }
    }
}
