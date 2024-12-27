using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Items.Inventory;

namespace Items {
    public class GrabbedItemProperties : MonoBehaviour
    {
        private static GrabbedItemProperties instance;
        public void Awake() {
            instance = this;
        }
        
        public ItemSlotUI ItemSlotUI;
        public ItemSlot ItemSlot {get => itemSlot;}
        public static GrabbedItemProperties Instance { get => instance;}
        private ItemSlot itemSlot;
        
        void Update()
        {
            Vector2 position = Input.mousePosition;
            transform.position = position;
        }
        public void SetItemSlot(ItemSlot itemSlot) {
            this.itemSlot = itemSlot;
            UpdateSprite();
        }

        public void UpdateSprite() {
            ItemSlotUI.Display(itemSlot);
        }
        
        public bool SetItemSlotFromInventory(List<ItemSlot> inventory, int n) {
            if (ItemSlot != null && ItemSlot.itemObject != null) {
                return false;
            }
            ItemSlot inventorySlot = inventory[n];
            ItemSlot newSlot = ItemSlotFactory.CreateNewItemSlot(inventorySlot.itemObject,1);
            inventorySlot.amount--;
            if (inventorySlot.amount == 0) {
                inventory[n] = null;
            }
            SetItemSlot(newSlot);
            return true;
        }

        public void AddItemSlotFromInventory(List<ItemSlot> inventory, int n) {
            ItemSlot inventorySlot = inventory[n];
            if (!ItemSlotHelper.AreEqual(ItemSlot,inventorySlot)) {
                return;
            }
            if (ItemSlot.amount >= Global.MaxSize) {
                return;
            }
            inventorySlot.amount--;
            if (inventorySlot.amount == 0) {
                inventory[n] = null;
            }
            this.ItemSlot.amount += 1;
            UpdateSprite();
        }
    }
}

