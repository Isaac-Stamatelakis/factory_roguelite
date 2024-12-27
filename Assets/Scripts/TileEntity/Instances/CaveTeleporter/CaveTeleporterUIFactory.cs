using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WorldModule.Caves;

namespace TileEntity.Instances {
    public static class CaveTeleporterUIFactory 
    {
        public static Button generateButton(Cave cave, GameObject prefab, Transform parent) {
            GameObject instantiated = GameObject.Instantiate(prefab);
            instantiated.name = "Button"+ cave.name;
            Transform childText = instantiated.transform.Find("Text");
            TextMeshProUGUI textMeshProUGUI = childText.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = cave.name;
            instantiated.transform.SetParent(parent,false);
            return instantiated.GetComponent<Button>();
        }
    }
}

