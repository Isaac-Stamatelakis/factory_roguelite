using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports.UI;

namespace ConduitModule.Ports {
    public class ConduitPortUIFactory
    {
        public static GameObject getUI(IConduit conduit, ConduitType conduitType) {
            GameObject prefab = null;
            switch (conduitType) {
                case ConduitType.Item:
                    prefab = Resources.Load<GameObject>("Prefabs/Ports/ItemPort");
                    break;
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            if (prefab == null) {
                Debug.LogError("ConduitPortUIFactory method 'getUI' prefab element was null for case '" + conduitType.ToString() + "'");
                return null;
            }
            GameObject instantiated = GameObject.Instantiate(prefab);
            switch (conduitType) {
                case ConduitType.Item:
                    instantiated.GetComponent<ItemConduitPortUIController>().initalize((ItemConduit)conduit);
                    break;
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            return instantiated;
        }

        public static Color getColorFromInt(int index) {
            switch (index)
            {
                case 0:
                    return new Color(0f, 1f, 1f, 1f); // Cyan
                case 1:
                    return new Color(0f, 1f, 0f, 1f); // Green
                case 2:
                    return new Color(1f, 0f, 0f, 1f); // Red
                case 3:
                    return new Color(1f, 1f, 0f, 1f); // Yellow
                case 4:
                    return new Color(1f, 0f, 1f, 1f); // Magenta
                case 5:
                    return new Color(0f, 0f, 1f, 1f); // Blue
                case 6:
                    return new Color(1f, 165f / 255f, 0f, 1f); // Orange
                case 7:
                    return new Color(0f, 1f, 128f / 255f, 1f); // Lime
                case 8:
                    return new Color(128f / 255f, 0f, 128f / 255f, 1f); // Purple
                case 9:
                    return new Color(0f, 128f / 255f, 128f / 255f, 1f); // Teal
                case 10:
                    return new Color(165f / 255f, 42f / 255f, 42f / 255f, 1f); // Brown
                case 11:
                    return new Color(1f, 192f / 255f, 203f / 255f, 1f); // Pink
                case 12:
                    return new Color(1f,1f,1f,1f);
                case 13:
                    return new Color(128f / 255f, 0f, 0f, 1f); // Maroon
                case 14:
                    return new Color(0f, 0f, 0f, 1f);
                case 15:
                    return new Color(128f / 255f, 128f / 255f, 128f / 255f, 1f); // Gray
                default:
                    return Color.gray;
            }
        }
    }
}

