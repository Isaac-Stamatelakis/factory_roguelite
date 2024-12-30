using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Entities;
using UI;

namespace TileEntity.Instances
{
    public class ChestInstance : TileEntityInstance<Chest>, IRightClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity, 
        IItemConduitInteractable, IPlaceInitializable, IConduitPortTileEntity
    {
        protected List<ItemSlot> items;
        public ChestInstance(Chest tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onBreak()
        {
            if (items == null) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to spawn items in unloaded chunk");
                return;
            }
            TileEntityHelper.spawnItemsOnBreak(items,getWorldPosition(),loadedChunk,loadedChunk.getSystem());
        }

        public void onRightClick()
        {
            GameObject uiElement = TileEntityObject.UIManager.getUIElement();
            if (uiElement == null) {
                Debug.LogError("GUI GameObject for chest:" + TileEntityObject.name + " null");
                return;
            }
            GameObject clone = GameObject.Instantiate(uiElement);
            InventoryUI inventoryUI = clone.GetComponent<InventoryUI>();
            inventoryUI.DisplayInventory(items);
            inventoryUI.SetRefresh(true);
            MainCanvasController.TInstance.DisplayUIWithPlayerInventory(clone);
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(items);
        }

        public void unserialize(string data)
        {
            items = ItemSlotFactory.Deserialize(data);
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public void PlaceInitialize()
        {
            items = new List<ItemSlot>();
            for (int i = 0; i < TileEntityObject.Rows*TileEntityObject.Columns;i++) {
                items.Add(null);
            }
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (ReferenceEquals(items[i]?.itemObject,null) || items[i].amount <= 0) continue;
                return items[i];
            }

            return null;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            ItemSlotHelper.InsertIntoInventory(items, toInsert, Global.MaxSize);
        }
    }
}
