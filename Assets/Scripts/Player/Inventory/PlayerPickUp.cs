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
        private int entityLayer;
        private RaycastHit2D[] hits;
        private const int MAX_ATTEMPTS_PER_UPDATE = 10;
       
        public void Start()
        {
            entityLayer = 1 << LayerMask.NameToLayer("Entity");
            playerInventory = gameObject.GetComponentInParent<PlayerInventory>();
        }

        private PlayerInventory playerInventory;

        public void FixedUpdate()
        {
            if (!CanPickUp) return;
            
            hits = Physics2D.CircleCastAll(transform.position, 0.5f,Vector2.zero, 0.25f, entityLayer);
            for (int i = 0; i < MAX_ATTEMPTS_PER_UPDATE; i++)
            {
                if (i >= hits.Length) break;
                if (hits[i].collider.gameObject.tag != "ItemEntity") continue;
                ItemEntity itemEntity = hits[i].transform.GetComponent<ItemEntity>();
                
                if (itemEntity.LifeTime < MINIMUM_PICKUP_TIME) continue;
                while (true)
                {
                    bool inserted = ItemSlotUtils.InsertIntoInventory(playerInventory.Inventory, itemEntity.itemSlot, Global.MaxSize);
                    if (!inserted) break;
                    if (!ItemSlotUtils.IsItemSlotNull(itemEntity.itemSlot)) continue;

                    GameObject.Destroy(itemEntity.gameObject);
                    playerInventory.Refresh();
                    break;
                }
            }
            
        }
    }
}
