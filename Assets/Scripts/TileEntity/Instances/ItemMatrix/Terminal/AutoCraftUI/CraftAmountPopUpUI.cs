using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;
using ItemModule.Inventory;
using UI;
using System;

namespace TileEntityModule.Instances.Matrix {
    public class CraftAmountPopUpUI : MonoBehaviour
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform amountIteratorContainer;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_InputField amountTextField;

        public void init(ItemMatrixController controller, ItemSlot toCraft, EncodedRecipe encodedRecipe) {
            amountTextField.text = "1";
            ItemSlotUIFactory.load(toCraft,itemContainer);
            AmountIteratorUI amountIteratorUI = AmountIteratorUI.newInstance();
            amountIteratorUI.init(amountIteratorContainer, amountTextField);
            cancelButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
            continueButton.onClick.AddListener(() => {
                AutoCraftPopupUI popupUI = AutoCraftPopupUI.newInstance();
                popupUI.init(controller,toCraft,Convert.ToInt32(amountTextField.text));
                GlobalUIContainer.getInstance().getUiController().addGUI(popupUI.gameObject);
                GameObject.Destroy(gameObject);
            });

        }

        public static CraftAmountPopUpUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/CraftAmountPopUp").GetComponent<CraftAmountPopUpUI>();
        }
    }
}

