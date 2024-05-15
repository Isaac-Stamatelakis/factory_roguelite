using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Items.Inventory;
using Items;

namespace TileEntityModule.Instances.Matrix {
    
    public class MatrixTerminalItemSlotUI : MonoBehaviour, IItemSlotUIElement, IPointerClickHandler
    {
        private ItemSlot itemSlot;
        private IMatrixTerminalItemClickReciever reciever;
        private int amount;
        private ItemObject itemObject;
        public void init(ItemSlot itemSlot, IMatrixTerminalItemClickReciever reciever){
            setItemSlot(itemSlot);
            this.reciever = reciever;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                reciever.itemLeftClick(this);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                reciever.itemRightClick(this);
            } else if (eventData.button == PointerEventData.InputButton.Middle) {
                reciever.itemMiddleClick(this);
            }
        }

        public static MatrixTerminalItemSlotUI newInstance(ItemSlot itemSlot, IMatrixTerminalItemClickReciever reciever) {
            MatrixTerminalItemSlotUI itemSlotUI = GlobalHelper.instantiateFromResourcePath("UI/Matrix/Terminal/MatrixTerminalItemUIPanel").GetComponent<MatrixTerminalItemSlotUI>();
            itemSlotUI.init(itemSlot, reciever);
            return itemSlotUI;
        }

        public ItemSlot getItemSlot()
        {
            return itemSlot;
        }

        public GameObject getGameObject()
        {
            return gameObject;
        }

        public void showCraftText() {
            ItemSlotUIFactory.replaceAmountTextWithString(transform,"Craft");
        }

        public void setItemSlot(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;
            if (itemSlot == null || itemSlot.itemObject == null) {
                this.itemObject = null;
                this.amount = -1;
            } else {
                this.itemObject = itemSlot.itemObject;
                this.amount = itemSlot.amount;
            }
        }

        public int getDisplayAmount()
        {
            return amount;
        }

        public ItemObject getDisplayItemObject()
        {
            return itemObject;
        }
    }
}

