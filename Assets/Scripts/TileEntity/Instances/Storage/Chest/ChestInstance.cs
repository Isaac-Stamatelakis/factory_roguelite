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
    public class ChestInstance : TileEntityInstance<Chest>, IRightClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity, IConduitTileEntity
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
            if (items == null) {
                initInventory();
            }
            /*
            GameObject shownGui = GameObject.Instantiate(uiElement);
            inventoryGrid.initalize(items, new Vector2Int((int) TileEntityObject.Rows, (int) TileEntityObject.Columns));
            MainCanvasController.Instance.DisplayObject(shownGui);
            */
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(items);
        }

        public void unserialize(string data)
        {
            if (data == null && items == null) {
                initInventory();
                return;
            } 
            this.items = ItemSlotFactory.Deserialize(data);
            
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }


        private void initInventory() {
            items = new List<ItemSlot>();
            for (int i = 0; i < TileEntityObject.Rows*TileEntityObject.Columns;i++) {
                items.Add(null);
            }
            
        }

        public void giveItems(List<ItemSlot> toGive) {
            if (items == null) {
                initInventory();
            }
            int j = 0;
            for (int i = 0; i < items.Count; i++) {
                if (j >= toGive.Count) {
                    return;
                }
                ItemSlot itemSlot = items[i];
                if (itemSlot == null || itemSlot.itemObject == null) {
                    items[i] = toGive[j];
                    j++;
                }
            }
        }
    }
}
