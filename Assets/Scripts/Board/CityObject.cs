using UnityEngine;

public class CityObject : MonoBehaviour
{
    private Renderer _renderer;

    public void Refresh(Building building)
    {
        var active = building is { Type: Building.BuildingType.City };

        gameObject.SetActive(active);

        if (active)
        {
            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<Renderer>();
            }

            if (_renderer != null)
            {
                _renderer.material.color = building.Owner.PlayerColor;
            }
        }
    }
}
