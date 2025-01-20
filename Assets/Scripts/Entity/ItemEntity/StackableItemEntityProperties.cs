using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
namespace Entities {
    public abstract class StackableItemEntity : ItemEntity
    {
        private float timeSinceLastUpdate;
        private float updateInterval = 0.25f;
        public bool amountMaxed() {
            return itemSlot.amount == Global.MaxSize;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag != "ItemEntity") return;
        
            StackableItemEntity itemEntity = other.gameObject.GetComponentInParent<StackableItemEntity>();
            MergeItemEntities(itemEntity);
        }

        /**
        Called on a successful raycast hit. 
        Will merge the TileItemEntities iff:
        they have the same id, same type, is not the object shotting the raycast, and is not maxed.
        Upon merging, will add the amounts if they do not go past the max, otherwise, one will be maxed the other will be left.
        **/
        private void MergeItemEntities(StackableItemEntity other) {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            
            ItemSlot hitObjectSlot = other.itemSlot;
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) return;
            
            ItemSlotUtils.InsertIntoSlot(itemSlot,hitObjectSlot,Global.MaxSize);
            if (ItemSlotUtils.IsItemSlotNull(hitObjectSlot)) Destroy(other.gameObject);
        }
    }
}


