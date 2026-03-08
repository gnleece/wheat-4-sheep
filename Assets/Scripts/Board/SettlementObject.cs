using UnityEngine;

public class SettlementObject : MonoBehaviour
{
    private new Renderer renderer;

    public void Refresh(Building building)
    {
        var active = building != null && building.Type == Building.BuildingType.Settlement;

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
