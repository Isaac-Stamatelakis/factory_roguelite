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
        private void combineStacks() {
            if (amountMaxed()) {
                return;
            }

            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate < updateInterval) {
                return;
            }
            timeSinceLastUpdate = 0;
            // Fires raycast from entity
            RaycastHit2D[] leftHits = Physics2D.RaycastAll(transform.position, Vector2.left, 0.5f, 1 << LayerMask.NameToLayer("Entity"));
            foreach (RaycastHit2D leftHit in leftHits) {
                mergeItemEntities(leftHit.collider.gameObject.GetComponents<StackableItemEntity>());
            }
            
            RaycastHit2D[] rightHits = Physics2D.RaycastAll(transform.position, Vector2.right, 0.5f, 1 << LayerMask.NameToLayer("Entity"));
            foreach (RaycastHit2D rightHit in rightHits) {
                mergeItemEntities(rightHit.collider.gameObject.GetComponents<StackableItemEntity>());
            }
        }

        /**
        Called on a successful raycast hit. 
        Will merge the TileItemEntities iff:
        they have the same id, same type, is not the object shotting the raycast, and is not maxed.
        Upon merging, will add the amounts if they do not go past the max, otherwise, one will be maxed the other will be left.
        **/
        private void mergeItemEntities(StackableItemEntity[] properties) {
            if (properties == null || properties.Length == 0) {
                return;
            }
            StackableItemEntity hitObjectProperties = (StackableItemEntity) properties[0];
            if (hitObjectProperties == null) {
                return;
            }
            if (itemSlot == null) {
                return;
            }
            ItemSlot hitObjectSlot = hitObjectProperties.itemSlot;
            
            if (
                hitObjectProperties != null 
                && hitObjectProperties.gameObject != gameObject
                && !hitObjectProperties.amountMaxed() 
                && itemSlot.itemObject.id == hitObjectSlot.itemObject.id 
                ) 
            {
                if (itemSlot.amount + hitObjectSlot.amount > Global.MaxSize) {
                    itemSlot.amount = Global.MaxSize;
                    hitObjectSlot.amount = Global.MaxSize-itemSlot.amount;
                } else {
                    itemSlot.amount += hitObjectSlot.amount;
                    this.lifeTime = (lifeTime + hitObjectProperties.LifeTime)/2;
                    Destroy(hitObjectProperties.gameObject);
                }
            }
        }
    }
}


