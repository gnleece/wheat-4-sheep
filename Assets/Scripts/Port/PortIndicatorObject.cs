using TMPro;
using UnityEngine;

/// <summary>
/// Attach to the port indicator prefab. Call Initialize() after instantiation to set the label.
/// </summary>
public class PortIndicatorObject : MonoBehaviour
{
    [SerializeField] private TextMeshPro label;

    public void Initialize(PortType portType)
    {
        if (label == null)
        {
            return;
        }

        label.text = GetPortLabel(portType);
    }

    private static string GetPortLabel(PortType portType)
    {
        switch (portType)
        {
            case PortType.Generic: return "3:1";
            case PortType.Wood:    return "2:1\nWood";
            case PortType.Clay:    return "2:1\nClay";
            case PortType.Sheep:   return "2:1\nSheep";
            case PortType.Wheat:   return "2:1\nWheat";
            case PortType.Ore:     return "2:1\nOre";
            default:               return "?";
        }
    }
}
