using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

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
                    avaiableText.text = "Avaiable: " + ItemDisplayUtils.formatAmountText(amount,oneInvisible:false);
                    if (missing <= 0) {
                        missingText.gameObject.SetActive(false);
                    } else {
                        missingText.text = "Missing: " + ItemDisplayUtils.formatAmountText(missing,oneInvisible:false);
                        panel.color = Color.red;
                    }
                    break;
                case AutoCraftElementMode.Output:
                    avaiableText.text = "To Craft: " + ItemDisplayUtils.formatAmountText(amount,oneInvisible:false);
                    missingText.gameObject.SetActive(false);
                    break;
            }
            ItemSlotUI itemSlotUI = ItemSlotUIFactory.newItemSlotUI(itemSlot,itemContainer,null);
            itemSlotUI.display(itemSlot);
        }
        public static AutoCraftItemUIElement newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/AutoCraftElement").GetComponent<AutoCraftItemUIElement>();   
        }
    }
}

