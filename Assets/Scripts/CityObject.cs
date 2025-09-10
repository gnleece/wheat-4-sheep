using UnityEngine;

public class CityObject : MonoBehaviour
{
    private new Renderer renderer;

    public void Refresh(Building building)
    {
        var active = building != null && building.Type == Building.BuildingType.City;

        gameObject.SetActive(active);

        if (active)
        {
            if (renderer == null)
            {
                renderer = GetComponentInChildren<Renderer>();
            }

            if (renderer != null)
            {
                renderer.material.color = building.Owner.PlayerColor;
            }
        }
    }
}
