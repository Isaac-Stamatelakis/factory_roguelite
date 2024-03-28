using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using ChunkModule;
using PlayerModule;
using ItemModule;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Storage/Drawer/Instance")]
    public class ItemDrawer : TileEntity, ILeftClickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, ILoadableTileEntity, IConduitInteractable, ISolidItemConduitInteractable, IBreakActionTileEntity
    {
        [SerializeField] private ConduitPortLayout conduitPortLayout;
        [SerializeField] private int maxStacks;
        private ItemSlot itemSlot;
        private SpriteRenderer visualElement;
        private float invincibilityFrames;

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            return itemSlot;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public void insertSolidItem(ItemSlot insert, Vector2Int portPosition)
        {
            ItemSlotHelper.insertIntoSlot(itemSlot, insert, maxStacks * Global.MaxSize);
        }

        public void load()
        {
            loadVisual();
        }

        private void loadVisual() {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                if (visualElement != null) {
                    visualElement.sprite = null;
                }
                return;
            } 
            if (visualElement == null) {
                GameObject visualElementObject = new GameObject();
                visualElementObject.name = itemSlot.itemObject.name + "_visual";
                visualElement = visualElementObject.AddComponent<SpriteRenderer>();
                visualElement.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
                visualElement.transform.position = getWorldPosition();    
            }
            
            visualElement.sprite = itemSlot.itemObject.getSprite();
            visualElement.transform.localScale = ItemSlotUIFactory.getConstrainedItemScale(visualElement.sprite,new Vector2(0.5f,0.5f));

        }

        public void onLeftClick()
        {
            if (!canInteract()) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            ItemSlot toDrop = ItemSlotFactory.createEmptyItemSlot();
            toDrop.itemObject = itemSlot.itemObject;
            
            toDrop.amount = Mathf.Min(Global.MaxSize,itemSlot.amount);
            itemSlot.amount -= toDrop.amount;
            if (itemSlot.amount <= 0) {
                itemSlot = null;
            }
            ItemEntityHelper.spawnItemEntity(getWorldPosition(),toDrop,loadedChunk.getEntityContainer());
            invincibilityFrames = 0.2f;
            loadVisual();
        }

        public bool canInteract() {
            invincibilityFrames -= Time.deltaTime;
            return invincibilityFrames <= 0f;
        }

        public void onRightClick()
        {
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();   
            ItemSlot playerItemSlot = playerInventory.getSelectedItemSlot();
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = playerItemSlot;
                playerInventory.removeSelectedItemSlot();
                loadVisual();
            } else {
                if (
                    ItemSlotHelper.areEqual(itemSlot,playerItemSlot) &&
                    ItemSlotHelper.canInsertIntoSlot(itemSlot,playerItemSlot,maxStacks*Global.MaxSize)
                ) {
                    ItemSlotHelper.insertIntoSlot(itemSlot,playerItemSlot,maxStacks*Global.MaxSize);
                    if (playerItemSlot.amount <= 0) {
                        playerInventory.removeSelectedItemSlot();
                        loadVisual();
                    }
                } 
            }
        }

        public string serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void unload()
        {
            if (visualElement != null) {
                GameObject.Destroy(visualElement.gameObject);
            }
        }

        public void unserialize(string data)
        {
            itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
        }

        public void onBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            while (itemSlot.amount > 0) {
                int amount = Mathf.Min(itemSlot.amount,Global.MaxSize);
                itemSlot.amount -= amount;
                ItemSlot drop = ItemSlotFactory.createNewItemSlot(itemSlot.itemObject,amount);
                ItemEntityHelper.spawnItemEntity(getWorldPosition(),drop,loadedChunk.getEntityContainer());
            }
        }

        public bool canBreak()
        {
            if (!canInteract()) {
                return false;
            }
            return itemSlot == null;
        }
    }
}

