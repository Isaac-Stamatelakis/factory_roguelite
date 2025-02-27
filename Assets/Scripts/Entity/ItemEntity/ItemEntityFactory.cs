using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Items;

namespace Entities {
    public static class ItemEntityFactory
    {
        public static GameObject SpawnItemEntity(Vector2 position, ItemSlot itemSlot, Transform entityContainer, Vector2? initialVelocity = null) {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot) || ReferenceEquals(itemSlot.itemObject.getSprite(),null)) {
                return null;
            }
            GameObject tileItemEntity = new GameObject();
            ItemEntity itemEntity = tileItemEntity.AddComponent<ItemEntity>();
            itemEntity.itemSlot = itemSlot;
            tileItemEntity.name = itemSlot.itemObject.name + "Entity";
            itemEntity.transform.position = new Vector3(position.x,position.y,0);
            itemEntity.transform.parent = entityContainer;
            itemEntity.tag = "ItemEntity";
            itemEntity.initalize();
            if (initialVelocity != null)
            {
                Rigidbody2D rb = itemEntity.GetComponent<Rigidbody2D>();
                rb.velocity = initialVelocity.Value;
            }
        
            return tileItemEntity;
        }

        public static GameObject SpawnItemEntityWithRandomVelocity(Vector2 position, ItemSlot itemSlot, Transform entityContainer)
        {
            Vector2 random = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(0, 1.0f));
            return SpawnItemEntityWithVelocity(position, itemSlot, entityContainer, random);
        }

        public static GameObject SpawnItemEntityWithVelocity(Vector2 position, ItemSlot itemSlot, Transform entityContainer, Vector2 velocity) {
            GameObject tileItemEntity = SpawnItemEntity(position,itemSlot,entityContainer);
            if (ReferenceEquals(tileItemEntity, null)) {
                return null;
            }
            tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
            return tileItemEntity;
        }

        public static GameObject SpawnItemEntityFromBreak(Vector2 position, ItemSlot itemSlot, Transform entityContainer) {
            GameObject tileItemEntity = SpawnItemEntity(position,itemSlot,entityContainer);
            if (ReferenceEquals(tileItemEntity,null)) {
                return null;
            }
            float randomX = Random.Range(-1f,1f);
            float randomY = Random.Range(0.5f,1f);
            Vector2 velocity = new Vector2(randomX,randomY).normalized;

            tileItemEntity.GetComponent<Rigidbody2D>().velocity = velocity;
            return tileItemEntity;
        }

        public static void spawnLootTable(Vector2 position, LootTable lootTable, Transform entityContainer) {
            List<ItemSlot> items = LootTableUtils.Open(lootTable);
            foreach (ItemSlot item in items) {
                SpawnItemEntityFromBreak(position,item,entityContainer);
            }
        }
    }

}

