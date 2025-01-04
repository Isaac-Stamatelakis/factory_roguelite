using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Chunks;
using PlayerModule;
using Items;
using Items.Inventory;
using Entities;
using Item.Slot;

namespace TileEntity.Instances.Storage {
    public class ItemDrawerInstance : TileEntityInstance<ItemDrawer>, ILeftClickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, ILoadableTileEntity, IConduitPortTileEntity, IItemConduitInteractable, IBreakActionTileEntity, ITileItemUpdateReciever
    {
        private ItemSlot itemSlot;
        private SpriteRenderer visualElement;
        private float invincibilityFrames;
        private DrawerControllerInstance controller;

        public ItemDrawerInstance(ItemDrawer tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public uint Amount {get => TileEntityObject.MaxStacks*Global.MaxSize;}

        public ItemSlot ItemSlot { get => itemSlot; }

        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
        {
            loadVisual();
            return itemSlot;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
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
            visualElement.transform.localScale = ItemDisplayUtils.getConstrainedItemScale(visualElement.sprite,new Vector2(0.5f,0.5f));

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
            toDrop.amount = GlobalHelper.MinUInt(Global.MaxSize, itemSlot.amount);
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
                    ItemSlotUtils.AreEqual(itemSlot,playerItemSlot) &&
                    ItemSlotUtils.CanInsertIntoSlot(itemSlot,playerItemSlot,TileEntityObject.MaxStacks*Global.MaxSize)
                ) {
                    ItemSlotUtils.InsertIntoSlot(itemSlot,playerItemSlot,TileEntityObject.MaxStacks*Global.MaxSize);
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
            while (itemSlot.amount > 0)
            {
                uint amount = GlobalHelper.MinUInt(itemSlot.amount, Global.MaxSize);
                itemSlot.amount -= amount;
                ItemSlot drop = ItemSlotFactory.CreateNewItemSlot(itemSlot.itemObject,(uint)amount);
                ItemEntityHelper.spawnItemEntity(getWorldPosition(),drop,loadedChunk.getEntityContainer());
            }
        }

        public bool canBreak()
        {
            if (!canInteract()) {
                return false;
            }
            return itemSlot == null || itemSlot.itemObject==null;
        }

        public void tileUpdate(TileItem tileItem)
        {
            if (controller == null) {
                return;
            }
            if (tileItem != null && !(tileItem.tileEntity != null && tileItem.tileEntity is ItemDrawer drawer)) {
                return;
            }
            controller.assembleMultiBlock();
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            throw new System.NotImplementedException();
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                itemSlot = ItemSlotFactory.Copy(toInsert);
                toInsert.amount = 0;
                loadVisual();
                return;
            }

            if (!ItemSlotUtils.CanInsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MaxSize))
                return;
            ItemSlotUtils.InsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MaxSize);
            loadVisual();
        }
    }
}

