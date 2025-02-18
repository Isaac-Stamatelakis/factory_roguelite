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
using TileEntity.MultiBlock;

namespace TileEntity.Instances.Storage {
    public class ItemDrawerInstance : TileEntityInstance<ItemDrawer>, ILeftClickableTileEntity, IRightClickableTileEntity, 
        ISerializableTileEntity, ILoadableTileEntity, IConduitPortTileEntity, IItemConduitInteractable, IBreakActionTileEntity, IMultiBlockTileAggregate
    {
        private const string SPRITE_SUFFIX = "_visual";
        private ItemSlot itemSlot;
        private SpriteRenderer visualElement;
        private float invincibilityFrames;
        private DrawerControllerInstance controller;

        public ItemDrawerInstance(ItemDrawer tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public uint Amount {get => TileEntityObject.MaxStacks*Global.MaxSize;}

        public ItemSlot ItemSlot { get => itemSlot; set => itemSlot = value; }
        

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public void Load()
        {
            LoadVisual();
        }

        public void LoadVisual() {
            if (chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                if (visualElement) visualElement.sprite = null;
                return;
            } 
            if (!visualElement) {
                GameObject visualElementObject = new GameObject();
                visualElementObject.name = itemSlot.itemObject.name + SPRITE_SUFFIX;
                visualElement = visualElementObject.AddComponent<SpriteRenderer>();
                visualElement.transform.SetParent(loadedChunk.getTileEntityContainer(),false);
                visualElement.transform.position = getWorldPosition();    
            }
            
            visualElement.sprite = itemSlot.itemObject.getSprite();
            visualElement.transform.localScale = ItemDisplayUtils.getConstrainedItemScale(visualElement.sprite,new Vector2(0.5f,0.5f));

        }

        public void OnLeftClick()
        {
            if (!CanInteract() || ItemSlotUtils.IsItemSlotNull(itemSlot) || chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            
            ItemSlot toDrop = ItemSlotFactory.createEmptyItemSlot();
            toDrop.itemObject = itemSlot.itemObject;
            toDrop.amount = GlobalHelper.MinUInt(Global.MaxSize, itemSlot.amount);
            itemSlot.amount -= toDrop.amount;
            if (itemSlot.amount <= 0) {
                itemSlot = null;
            }
            ItemEntityFactory.SpawnItemEntity(getWorldPosition(),toDrop,loadedChunk.getEntityContainer());
            invincibilityFrames = 0.2f;
            LoadVisual();
        }

        public bool CanInteract() {
            invincibilityFrames -= Time.deltaTime;
            return invincibilityFrames <= 0f;
        }

        public void OnRightClick()
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            ItemSlot playerItemSlot = playerInventory.getSelectedItemSlot();
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                itemSlot = playerItemSlot;
                playerInventory.removeSelectedItemSlot();
                LoadVisual();
                return;
            }

            if (!ItemSlotUtils.AreEqual(itemSlot, playerItemSlot) || !ItemSlotUtils.CanInsertIntoSlot(itemSlot,
                    playerItemSlot, TileEntityObject.MaxStacks * Global.MaxSize)) return;
            
            ItemSlotUtils.InsertIntoSlot(itemSlot, playerItemSlot, TileEntityObject.MaxStacks * Global.MaxSize);
            if (playerItemSlot.amount > 0) return;
            
            playerInventory.removeSelectedItemSlot();
            LoadVisual();


        }

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void Unload()
        {
            if (visualElement != null) {
                GameObject.Destroy(visualElement.gameObject);
            }
        }

        public void Unserialize(string data)
        {
            itemSlot = ItemSlotFactory.DeserializeSlot(data);
        }

        public void OnBreak()
        {
            if (chunk is not ILoadedChunk loadedChunk || ReferenceEquals(itemSlot,null)) {
                return;
            }
            
            while (itemSlot.amount > 0)
            {
                uint amount = GlobalHelper.MinUInt(itemSlot.amount, Global.MaxSize);
                itemSlot.amount -= amount;
                ItemSlot drop = ItemSlotFactory.CreateNewItemSlot(itemSlot.itemObject,(uint)amount);
                ItemEntityFactory.SpawnItemEntity(getWorldPosition(),drop,loadedChunk.getEntityContainer());
            }
        }

        public bool CanBreak()
        {
            if (!CanInteract()) {
                return false;
            }

            return ItemSlotUtils.IsItemSlotNull(itemSlot);
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            return itemSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                itemSlot = ItemSlotFactory.Copy(toInsert);
                toInsert.amount = 0;
                LoadVisual();
                return;
            }

            if (!ItemSlotUtils.CanInsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MaxSize))
                return;
            ItemSlotUtils.InsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MaxSize);
            LoadVisual();
        }

        public IMultiBlockTileEntity GetAggregator()
        {
            return controller;
        }

        public void SetAggregator(IMultiBlockTileEntity aggregator)
        {
            Debug.Log("HI");
            if (aggregator is not DrawerControllerInstance drawerControllerInstance) return;
            controller = drawerControllerInstance;
        }
    }
}

