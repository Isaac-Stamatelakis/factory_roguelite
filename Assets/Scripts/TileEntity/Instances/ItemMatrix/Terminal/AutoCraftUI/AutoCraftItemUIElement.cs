using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Matrix {
    public class AutoCraftItemUIElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Transform itemContainer;
        public void init() {
            
        }
        public AutoCraftItemUIElement newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/AutoCraftElement").GetComponent<AutoCraftItemUIElement>();   
        }
    }
}

