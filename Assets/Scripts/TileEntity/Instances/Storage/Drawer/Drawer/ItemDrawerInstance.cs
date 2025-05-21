using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Chunks;
using PlayerModule;
using Items;
using Items.Inventory;
using Entities;
using Item.Display;
using Item.Slot;
using Items.Transmutable;
using TileEntity.MultiBlock;

namespace TileEntity.Instances.Storage {
    public class ItemDrawerInstance : TileEntityInstance<ItemDrawer>, ILeftClickableTileEntity, IRightClickableTileEntity, 
        ISerializableTileEntity, ILoadableTileEntity, IConduitPortTileEntity, IItemConduitInteractable, IBreakActionTileEntity, IMultiBlockTileAggregate, IWorldToolTipTileEntity
    {
        private const string SPRITE_SUFFIX = "_visual";
        private ItemSlot itemSlot;
        private ItemWorldDisplay visualElement;
        private float invincibilityFrames;
        private DrawerControllerInstance controller;

        public ItemDrawerInstance(ItemDrawer tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public uint Amount {get => TileEntityObject.MaxStacks*Global.MAX_SIZE;}

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
            
            if (ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                if (visualElement)
                {
                    Object.Destroy(visualElement.gameObject);
                }
                visualElement = null;
                return;
            } 
            
            if (!visualElement) {
                GameObject visualElementObject = new GameObject();
                visualElement = visualElementObject.AddComponent<ItemWorldDisplay>();
                visualElementObject.name = itemSlot.itemObject.name + SPRITE_SUFFIX;
                visualElement.transform.SetParent(loadedChunk.GetTileEntityContainer(),false);
                Vector2 worldPosition = GetWorldPosition();
                visualElement.transform.position = new Vector3(worldPosition.x, worldPosition.y, -1); // Set z to -1 so tile highlighter doesn't hide visual element 
            }
            else
            {
                visualElement.gameObject.SetActive(true);
                visualElement.enabled = true;
            }
            visualElement.Display(itemSlot);
            

            const float TILE_COVER_RATIO = 0.7f;

            Sprite visualElementSprite = visualElement.GetComponent<SpriteRenderer>().sprite;
            if (!visualElementSprite) // GetComponentInChildren<SpriteRenderer>() is not working in this case for some reason
            {
                foreach (Transform child in visualElement.transform)
                {
                    visualElementSprite = child.GetComponent<SpriteRenderer>().sprite;
                    if (visualElementSprite) break;
                }
            }
            if (!visualElementSprite) return;
            
            visualElement.transform.localScale = TILE_COVER_RATIO * ItemDisplayUtils.GetSpriteRenderItemScale(visualElementSprite);

        }
        

        public void OnLeftClick()
        {
            if (!CanInteract() || ItemSlotUtils.IsItemSlotNull(itemSlot) || chunk is not ILoadedChunk loadedChunk) {
                return;
            }
            
            ItemSlot toDrop = ItemSlotFactory.createEmptyItemSlot();
            toDrop.itemObject = itemSlot.itemObject;
            toDrop.amount = GlobalHelper.MinUInt(Global.MAX_SIZE, itemSlot.amount);
            itemSlot.amount -= toDrop.amount;
            if (itemSlot.amount <= 0) {
                itemSlot = null;
            }
            ItemEntityFactory.SpawnItemEntity(GetWorldPosition(),toDrop,loadedChunk.GetEntityContainer());
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
                OnInsertUpdate();
                return;
            }

            if (!ItemSlotUtils.AreEqual(itemSlot, playerItemSlot) || !ItemSlotUtils.CanInsertIntoSlot(itemSlot, playerItemSlot, TileEntityObject.MaxStacks * Global.MAX_SIZE)) return;
            
            ItemSlotUtils.InsertIntoSlot(itemSlot, playerItemSlot, TileEntityObject.MaxStacks * Global.MAX_SIZE);
            if (playerItemSlot.amount > 0) return;
            OnInsertUpdate();
            return;

            void OnInsertUpdate()
            {
                playerInventory.RemoveSelectedItemSlot();
                playerInventory.RefreshSelectedSlotDisplay();
                LoadVisual();
            }
        }

        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void Unload()
        {
            if (visualElement) {
                Object.Destroy(visualElement.gameObject);
            }
            visualElement = null;
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
                uint amount = GlobalHelper.MinUInt(itemSlot.amount, Global.MAX_SIZE);
                itemSlot.amount -= amount;
                ItemSlot drop = ItemSlotFactory.CreateNewItemSlot(itemSlot.itemObject,(uint)amount);
                ItemEntityFactory.SpawnItemEntity(GetWorldPosition(),drop,loadedChunk.GetEntityContainer());
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

            if (!ItemSlotUtils.CanInsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MAX_SIZE))
                return;
            ItemSlotUtils.InsertIntoSlot(itemSlot, toInsert, TileEntityObject.MaxStacks * Global.MAX_SIZE);
            LoadVisual();
        }

        public IMultiBlockTileEntity GetAggregator()
        {
            return controller;
        }

        public void SetAggregator(IMultiBlockTileEntity aggregator)
        {
            if (aggregator is not DrawerControllerInstance drawerControllerInstance) return;
            controller = drawerControllerInstance;
        }

        public string GetTextPreview()
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return "Storing Nothing";
            return $"Storing {ItemDisplayUtils.FormatAmountText(itemSlot.amount)} of {itemSlot.itemObject.name}";
        }
    }
}

