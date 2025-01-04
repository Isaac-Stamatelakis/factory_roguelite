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
using Item.Slot;

namespace TileEntity.Instances.Matrix {
    public class CraftAmountPopUpUI : MonoBehaviour, IAmountIteratorListener
    {
        [SerializeField] private ItemSlotUI ItemSlotUIPrefab;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform amountIteratorContainer;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_InputField amountTextField;
        [SerializeField] private AmountIteratorUI amountIteratorUI;

        public void init(ItemMatrixControllerInstance controller, ItemSlot toCraft, EncodedRecipe encodedRecipe) {
            ItemSlotUI itemSlotUI = Instantiate(ItemSlotUIPrefab, itemContainer);
            itemSlotUI.Display(toCraft);

            amountIteratorUI.setListener(this);

            cancelButton.onClick.AddListener(() => {
                CanvasController.Instance.PopStack();
            });
            continueButton.onClick.AddListener(() => {
                AutoCraftPopupUI popupUI = AutoCraftPopupUI.newInstance();
                popupUI.init(controller,toCraft,Convert.ToUInt32(amountTextField.text));
                CanvasController.Instance.PopStack();
                CanvasController.Instance.DisplayObject(popupUI.gameObject);
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

