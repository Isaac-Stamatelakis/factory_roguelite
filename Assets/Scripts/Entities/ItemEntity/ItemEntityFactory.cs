using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using UnityEngine;
using Items;
using Items.Tags;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Entities {
    public static class ItemEntityFactory
    {
        public static void SpawnItemEntities(Vector2 position, List<ItemSlot> itemSlots, Transform entityContainer,
            Vector2? initialVelocity = null)
        {
            foreach (ItemSlot itemSlot in itemSlots)
            {
                SpawnItemEntity(position, itemSlot, entityContainer, initialVelocity);
            }
        }
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
            if (itemSlot.tags?.Dict != null)
            {
                SpawnTagObjects(itemEntity, itemSlot);
            }

            if (itemSlot.itemObject is TileItem tileItem)
            {
                var tileOverlay = tileItem.tileOptions.Overlay;
                if (tileOverlay)
                {
                    AddOverlaySprite(itemEntity,TileItem.GetDefaultSprite(tileOverlay.GetDisplayTile()),tileOverlay.GetColor());
                }
            }

            if (itemSlot.itemObject.SpriteOverlays != null)
            {
                foreach (SpriteOverlay spriteOverlay in itemSlot.itemObject.SpriteOverlays)
                {
                    AddOverlaySprite(itemEntity,spriteOverlay.Sprite,spriteOverlay.Color);
                }
            }
            
            if (initialVelocity != null)
            {
                Rigidbody2D rb = itemEntity.GetComponent<Rigidbody2D>();
                rb.velocity = initialVelocity.Value;
            }
        
            return tileItemEntity;
        }

        private static void AddOverlaySprite(ItemEntity itemEntity, Sprite sprite, Color color)
        {
            GameObject overlayContainer = new GameObject("SpriteOverlay");
            SpriteRenderer spriteRenderer = overlayContainer.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            overlayContainer.transform.SetParent(itemEntity.transform,false);
            overlayContainer.transform.localPosition = new Vector3(0, 0, -0.1f);
        }

        private static void SpawnTagObjects(ItemEntity itemEntity, ItemSlot itemSlot)
        {
            foreach (var (tag, data) in itemSlot.tags.Dict)
            {
                GameObject gameObject = tag.GetWorldTagElement(itemSlot,data);
                if (!gameObject) continue;
                ItemTagVisualLayer visualLayer = tag.GetVisualLayer();
                gameObject.transform.SetParent(itemEntity.transform,false);
                switch (visualLayer)
                {
                    case ItemTagVisualLayer.Front:
                        gameObject.transform.SetAsFirstSibling();
                        break;
                    case ItemTagVisualLayer.Back:
                        gameObject.transform.SetAsLastSibling();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
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

