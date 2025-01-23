using System;
using System.Collections;
using System.Collections.Generic;
using Entities;
using Item.Slot;
using PlayerModule;
using UnityEngine;

namespace Player.Inventory
{
    /// <summary>
    /// Handles item entity pickup for player
    /// </summary>
    public class PlayerPickUp : MonoBehaviour
    {
        private const float MINIMUM_PICKUP_TIME = 0.5f;
        public bool CanPickUp = true;
        
        private List<ItemEntity> collidedItemEntities = new List<ItemEntity>();
        public void Start()
        {
            playerInventory = gameObject.GetComponentInParent<PlayerInventory>();
        }

        private PlayerInventory playerInventory;
        
        /// <summary>
        /// Tries to insert all items the player is currently collided into the players inventory.
        /// Items collided with first are tried first
        /// This is called whenever the player inventory has an update event
        /// </summary>
        public void TryPickUpAllCollided()
        {
            for (int i = collidedItemEntities.Count - 1; i >= 0; i--)
            {
                bool destroyed = TryInsert(collidedItemEntities[i]);
                if (!destroyed) continue;
                collidedItemEntities.RemoveAt(i);
                i--;
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
            ItemEntity itemEntity = other.GetComponent<ItemEntity>();
            
            if (TryInsert(itemEntity)) return;
            collidedItemEntities.Add(itemEntity);
            
            if (itemEntity.LifeTime < MINIMUM_PICKUP_TIME && CanPickUp)
            {
                StartCoroutine(DelayedPickup(itemEntity, MINIMUM_PICKUP_TIME - itemEntity.LifeTime));
            }
        }
        
        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
            ItemEntity itemEntity = other.GetComponent<ItemEntity>();
            if (collidedItemEntities.Remove(itemEntity)) TryInsert(itemEntity);
        }


        private bool TryInsert(ItemEntity itemEntity)
        {
            if (!CanPickUp) return false;
            while (true)
            {
                if (itemEntity.LifeTime < MINIMUM_PICKUP_TIME) return false;

                bool inserted = ItemSlotUtils.InsertIntoInventory(playerInventory.Inventory, itemEntity.itemSlot, Global.MaxSize);
                if (!inserted) return false;
                if (!ItemSlotUtils.IsItemSlotNull(itemEntity.itemSlot)) continue;

                GameObject.Destroy(itemEntity.gameObject);
                playerInventory.Refresh();

                return true;
            }
        }

        private IEnumerator DelayedPickup(ItemEntity itemEntity, float delay)
        {
            yield return new WaitForSeconds(delay + 0.05f);
            
            if (!collidedItemEntities.Contains(itemEntity)) yield break;
            
            if (TryInsert(itemEntity)) collidedItemEntities.Remove(itemEntity);
        }
    }
}
