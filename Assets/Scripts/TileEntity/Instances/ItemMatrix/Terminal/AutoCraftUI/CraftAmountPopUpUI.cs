using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using Items.Inventory;
using UI;
using System.Threading.Tasks;
using System;

namespace TileEntityModule.Instances.Matrix {
    public class CraftAmountPopUpUI : MonoBehaviour, IAmountIteratorListener
    {
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform amountIteratorContainer;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_InputField amountTextField;
        [SerializeField] private AmountIteratorUI amountIteratorUI;

        public void init(ItemMatrixControllerInstance controller, ItemSlot toCraft, EncodedRecipe encodedRecipe) {
            amountTextField.text = "1";
            ItemSlotUI itemSlotUI = ItemSlotUIFactory.newItemSlotUI(toCraft,itemContainer,null);

            amountIteratorUI.setListener(this);

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

        public void iterate(int amount)
        {
            int current = Convert.ToInt32(amountTextField.text);
            current += amount;
            amountTextField.text = current.ToString();
        }
    }
}

