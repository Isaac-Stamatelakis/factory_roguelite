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
            itemSlotUI = gameObject.AddComponent<ItemSlotUI>();
            itemSlotUI.init(null);
        }
        
        private ItemSlotUI itemSlotUI;
        public ItemSlot ItemSlot {get => itemSlot;}
        public static GrabbedItemProperties Instance { get => instance;}
        private ItemSlot itemSlot;
        public void FixedUpdate() {
            itemSlotUI.display(itemSlot);
        }
        void Update()
        {
            Vector3 position = Input.mousePosition;
            position.z = 0;
            transform.position = position;
        }
        public void setItemSlot(ItemSlot itemSlot) {
            this.itemSlot = itemSlot;
            updateSprite();
        }

        public void updateSprite() {
            itemSlotUI.display(itemSlot);
        }
        
        public bool setItemSlotFromInventory(List<ItemSlot> inventory, int n) {
            if (ItemSlot != null && ItemSlot.itemObject != null) {
                return false;
            }
            ItemSlot inventorySlot = inventory[n];
            ItemSlot newSlot = ItemSlotFactory.createNewItemSlot(inventorySlot.itemObject,1);
            inventorySlot.amount--;
            if (inventorySlot.amount == 0) {
                inventory[n] = null;
            }
            setItemSlot(newSlot);
            return true;
        }

        public void addItemSlotFromInventory(List<ItemSlot> inventory, int n) {
            ItemSlot inventorySlot = inventory[n];
            if (!ItemSlotHelper.areEqual(ItemSlot,inventorySlot)) {
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
            updateSprite();
        }
    }
}

