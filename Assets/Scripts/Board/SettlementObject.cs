using UnityEngine;

public class SettlementObject : MonoBehaviour
{
    private Renderer _renderer;

    public void Refresh(Building building)
    {
        var active = building != null && building.Type == Building.BuildingType.Settlement;

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
