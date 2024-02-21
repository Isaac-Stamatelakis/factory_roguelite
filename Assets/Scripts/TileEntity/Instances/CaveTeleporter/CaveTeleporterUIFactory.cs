using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TileEntityModule.Instances {
    public static class CaveTeleporterUIFactory 
    {
        public static Button getButton(CaveRegion caveRegion, GameObject prefab, Transform parent) {
            GameObject instantiated = GameObject.Instantiate(prefab);
            instantiated.name = "Button"+ caveRegion.ToString();
            Transform childText = instantiated.transform.Find("Text");
            TextMeshProUGUI textMeshProUGUI = childText.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = caveRegion.ToString();
            instantiated.transform.SetParent(parent,false);
            return instantiated.GetComponent<Button>();
        }
    }
}

