using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports.UI;
using UnityEngine.UI;
using TMPro;

namespace ConduitModule.Ports {
    public class ConduitPortUIFactory
    {
        public static GameObject getUI(IPortConduit conduit, ConduitType conduitType, EntityPortType portType) {

            // Base prefab which is used on all ports
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Ports/BasePort");
            GameObject instantiated = GameObject.Instantiate(prefab);
            BaseConduitPortUIController baseConduitPortUIController = instantiated.GetComponent<BaseConduitPortUIController>();
            baseConduitPortUIController.initalize(conduit);
            if (portType != EntityPortType.All) {
                if (portType != EntityPortType.Input) {
                    baseConduitPortUIController.toggleInsertCover();
                    baseConduitPortUIController.toggleColorInsertButton.image.color = Color.black;
                    baseConduitPortUIController.toggleInsertButton.image.color = Color.black;
                }
                if (portType != EntityPortType.Output) {
                    baseConduitPortUIController.toggleExtractCover();
                    baseConduitPortUIController.toggleExtractButton.image.color = Color.black;
                    baseConduitPortUIController.toggleColorExtractButton.image.color = Color.black;
                }
            }
            // Decorate ui
            if (conduitType == ConduitType.Item || conduitType == ConduitType.Fluid) {
                if (conduitType == ConduitType.Item) {
                    baseConduitPortUIController.setBackground(Resources.Load<Sprite>("Sprites/Port/green_grad"));
                } else if (conduitType == ConduitType.Fluid) {
                    baseConduitPortUIController.setBackground(Resources.Load<Sprite>("Sprites/Port/fluid_conduit"));
                }
                
                ItemConduitPortUIController itemConduitPortUIController = instantiated.AddComponent<ItemConduitPortUIController>();

                GameObject roundRobin = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Ports/ExtractRoundContainer"));
                roundRobin.transform.SetParent(instantiated.transform);
                itemConduitPortUIController.roundRobinButton = roundRobin.transform.Find("Button").GetComponent<Button>();

                GameObject priority = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Ports/Priority"));
                priority.transform.SetParent(instantiated.transform);
                itemConduitPortUIController.priorityText = priority.transform.Find("Number").GetComponent<TextMeshProUGUI>();
                itemConduitPortUIController.plusPriorityButton = priority.transform.Find("Plus").Find("Button").GetComponent<Button>();
                itemConduitPortUIController.minusPriorityButton = priority.transform.Find("Minus").Find("Button").GetComponent<Button>();
                itemConduitPortUIController.initalize(conduit);
            }
            else if (conduitType == ConduitType.Energy) {
                EnergyConduitPortUIController energyConduitPortUIController = instantiated.AddComponent<EnergyConduitPortUIController>();

                baseConduitPortUIController.setBackground(Resources.Load<Sprite>("Sprites/Port/energy_back"));

                GameObject priority = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Ports/Priority"));
                priority.transform.SetParent(instantiated.transform);
                energyConduitPortUIController.priorityText = priority.transform.Find("Number").GetComponent<TextMeshProUGUI>();
                energyConduitPortUIController.plusPriorityButton = priority.transform.Find("Plus").Find("Button").GetComponent<Button>();
                energyConduitPortUIController.minusPriorityButton = priority.transform.Find("Minus").Find("Button").GetComponent<Button>();
                energyConduitPortUIController.initalize(conduit);
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

