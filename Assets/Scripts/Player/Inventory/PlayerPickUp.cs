using System;
using Entities;
using Item.Slot;
using PlayerModule;
using UnityEngine;

namespace Player.Inventory
{
    public class PlayerPickUp : MonoBehaviour
    {
        public void Start()
        {
            playerInventory = gameObject.GetComponentInParent<PlayerInventory>();
        }

        private PlayerInventory playerInventory;

        public void OnTriggerEnter2D(Collider2D other)
        {
            TryInsert(other);
        }
        
        public void OnTriggerExit2D(Collider2D other)
        {
            TryInsert(other);
        }

        private void TryInsert(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
        
            ItemEntity itemEntity = other.gameObject.GetComponent<ItemEntity>();
            bool inserted = ItemSlotUtils.InsertIntoInventory(playerInventory.Inventory, itemEntity.itemSlot, Global.MaxSize);
            if (!inserted) return;
        
            GameObject.Destroy(itemEntity.gameObject);
            playerInventory.Refresh();
        }
    
    }
}
