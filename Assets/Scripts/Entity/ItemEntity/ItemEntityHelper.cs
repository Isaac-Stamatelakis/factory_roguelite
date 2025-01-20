using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Items;

namespace Entities {
    public static class ItemEntityHelper
    {
        public static GameObject spawnItemEntity(Vector2 position, ItemSlot itemSlot, Transform entityContainer) {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                return null;
            }
            GameObject tileItemEntity = new GameObject();
            tileItemEntity.AddComponent<TileItemEntityProperties>();
            TileItemEntityProperties itemProperties = tileItemEntity.GetComponent<TileItemEntityProperties>();
            itemProperties.itemSlot = itemSlot;
            tileItemEntity.name = itemSlot.itemObject.name + "Entity";
            itemProperties.transform.position = new Vector3(position.x,position.y,0);
            itemProperties.transform.parent = entityContainer;
            itemProperties.tag = "ItemEntity";
            itemProperties.initalize();

            GameObject trigger = new GameObject();
            trigger.name = "Trigger";
            BoxCollider2D collider2D = trigger.AddComponent<BoxCollider2D>();
            collider2D.size = new Vector2(0.5f, 0.5f);
            collider2D.isTrigger = true;
            trigger.transform.SetParent(tileItemEntity.transform,false);
            return tileItemEntity;
        }

        public static GameObject spawnItemEntityWithVelocity(Vector2 position, ItemSlot itemSlot, Transform entityContainer, Vector2 velocity) {
            GameObject tileItemEntity = spawnItemEntity(position,itemSlot,entityContainer);
            if (ReferenceEquals(tileItemEntity, null)) {
                return null;
            }
            tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
            return tileItemEntity;
        }

        public static GameObject spawnItemEntityFromBreak(Vector2 position, ItemSlot itemSlot, Transform entityContainer) {
            GameObject tileItemEntity = spawnItemEntity(position,itemSlot,entityContainer);
            if (tileItemEntity == null) {
                return null;
            }
            float randomX = Random.Range(-1f,1f);
            float randomY = Random.Range(0.5f,1f);
            Vector2 velocity = new Vector2(randomX,randomY).normalized;

            tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
            return tileItemEntity;
        }

        public static void spawnLootTable(Vector2 position, LootTable lootTable, Transform entityContainer) {
            List<ItemSlot> items = LootTableHelper.open(lootTable);
            foreach (ItemSlot item in items) {
                spawnItemEntityFromBreak(position,item,entityContainer);
            }
        }
    }

}

