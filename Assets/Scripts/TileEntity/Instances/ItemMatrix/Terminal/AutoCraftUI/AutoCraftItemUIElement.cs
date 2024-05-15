using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;

namespace TileEntityModule.Instances.Matrix {
    public enum AutoCraftElementMode {
        Input,
        Output
    }
    public class AutoCraftItemUIElement : MonoBehaviour
    {
        [SerializeField] private Image panel;
        [SerializeField] private TextMeshProUGUI avaiableText;
        [SerializeField] private TextMeshProUGUI missingText;
        [SerializeField] private Transform itemContainer;
        public void init(AutoCraftElementMode mode, ItemSlot itemSlot, int amount, int required) {
            int missing = required-amount;
            switch (mode) {
                case AutoCraftElementMode.Input:
                    avaiableText.text = "Avaiable: " + ItemSlotUIFactory.formatAmountText(amount,oneInvisible:false);
                    if (missing <= 0) {
                        missingText.gameObject.SetActive(false);
                    } else {
                        missingText.text = "Missing: " + ItemSlotUIFactory.formatAmountText(missing,oneInvisible:false);
                        panel.color = Color.red;
                    }
                    break;
                case AutoCraftElementMode.Output:
                    avaiableText.text = "To Craft: " + ItemSlotUIFactory.formatAmountText(amount,oneInvisible:false);
                    missingText.gameObject.SetActive(false);
                    break;
            }
            ItemSlotUIFactory.load(itemSlot,itemContainer);
        }
        public static AutoCraftItemUIElement newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/AutoCraftElement").GetComponent<AutoCraftItemUIElement>();   
        }
    }
}

