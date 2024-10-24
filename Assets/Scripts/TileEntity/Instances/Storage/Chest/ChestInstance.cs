using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Entities;

namespace TileEntityModule.Instances
{
    public class ChestInstance : TileEntityInstance<Chest>, IRightClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity, IConduitInteractable
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
            GameObject uiElement = tileEntity.UIManager.getUIElement();
            if (uiElement == null) {
                Debug.LogError("GUI GameObject for chest:" + tileEntity.name + " null");
                return;
            }
            if (items == null) {
                initInventory();
            }
            GlobalUIController tileEntityGUIController = GlobalUIContainer.getInstance().getUiController();
            GameObject shownGui = GameObject.Instantiate(uiElement);
            SolidDynamicInventory inventoryGrid = shownGui.GetComponent<SolidDynamicInventory>();
            inventoryGrid.initalize(items, new Vector2Int((int) tileEntity.Rows, (int) tileEntity.Columns));
            tileEntityGUIController.setGUI(shownGui);
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
            this.items = ItemSlotFactory.deserialize(data);
            
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitLayout;
        }


        private void initInventory() {
            items = new List<ItemSlot>();
            for (int i = 0; i < tileEntity.Rows*tileEntity.Columns;i++) {
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
