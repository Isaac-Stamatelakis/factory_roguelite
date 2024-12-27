using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Items.Inventory;
using Items;

namespace TileEntity.Instances.Matrix {
    
    public class MatrixTerminalItemSlotUI : MonoBehaviour, IItemSlotUIElement, IPointerClickHandler
    {
        private ItemSlot itemSlot;
        private IMatrixTerminalItemClickReciever reciever;
        private uint amount;
        private ItemObject itemObject;
        public void init(ItemSlot itemSlot, IMatrixTerminalItemClickReciever reciever){
            SetItemSlot(itemSlot);
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

        public ItemSlot GetItemSlot()
        {
            return itemSlot;
        }
        
        public void ShowCraftText() {
            GetComponent<ItemSlotUI>().SetText("Craft");
        }

        public void SetItemSlot(ItemSlot itemSlot)
        {
            this.itemSlot = itemSlot;
            if (itemSlot == null || itemSlot.itemObject == null) {
                this.itemObject = null;
                this.amount = 0;
            } else {
                this.itemObject = itemSlot.itemObject;
                this.amount = itemSlot.amount;
            }
        }

        public uint GetDisplayAmount()
        {
            return amount;
        }

        public ItemObject GetDisplayItemObject()
        {
            return itemObject;
        }

        public ItemSlotUI GetItemSlotUI()
        {
            return GetComponent<ItemSlotUI>();
        }
    }
}

