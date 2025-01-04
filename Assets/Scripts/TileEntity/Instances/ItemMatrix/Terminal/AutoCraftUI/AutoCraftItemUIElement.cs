using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

namespace TileEntity.Instances.Matrix {
    public enum AutoCraftElementMode {
        Input,
        Output
    }
    public class AutoCraftItemUIElement : MonoBehaviour
    {
        [SerializeField] private ItemSlotUI ItemSlotUIPrefab;
        [SerializeField] private Image panel;
        [SerializeField] private TextMeshProUGUI avaiableText;
        [SerializeField] private TextMeshProUGUI missingText;
        [SerializeField] private Transform itemContainer;
        public void init(AutoCraftElementMode mode, ItemSlot itemSlot, uint amount, uint required) {
            uint missing = required-amount;
            switch (mode) {
                case AutoCraftElementMode.Input:
                    avaiableText.text = "Avaiable: " + ItemDisplayUtils.FormatAmountText(amount,oneInvisible:false);
                    if (missing <= 0) {
                        missingText.gameObject.SetActive(false);
                    } else {
                        missingText.text = "Missing: " + ItemDisplayUtils.FormatAmountText(missing,oneInvisible:false);
                        panel.color = Color.red;
                    }
                    break;
                case AutoCraftElementMode.Output:
                    avaiableText.text = "To Craft: " + ItemDisplayUtils.FormatAmountText(amount,oneInvisible:false);
                    missingText.gameObject.SetActive(false);
                    break;
            }

            ItemSlotUI itemSlotUI = Instantiate(ItemSlotUIPrefab, itemContainer);
            itemSlotUI.Display(itemSlot);
        }
        public static AutoCraftItemUIElement newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/AutoCraftElement").GetComponent<AutoCraftItemUIElement>();   
        }
    }
}

