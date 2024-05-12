using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;

namespace Entities {
    public static class ItemEntityHelper
    {
        public static GameObject spawnItemEntity(Vector2 position, ItemSlot itemSlot, Transform entityContainer) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return null;
            }
            GameObject tileItemEntity = new GameObject();
            tileItemEntity.AddComponent<TileItemEntityProperties>();
            TileItemEntityProperties itemProperties = tileItemEntity.GetComponent<TileItemEntityProperties>();
            itemProperties.itemSlot = itemSlot;
            itemProperties.transform.position = new Vector3(position.x,position.y,0);
            itemProperties.transform.parent = entityContainer;
            itemProperties.initalize();
            return tileItemEntity;
        }

        public static GameObject spawnItemEntityWithVelocity(Vector2 position, ItemSlot itemSlot, Transform entityContainer, Vector2 velocity) {
            GameObject tileItemEntity = spawnItemEntity(position,itemSlot,entityContainer);
            if (tileItemEntity == null) {
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

