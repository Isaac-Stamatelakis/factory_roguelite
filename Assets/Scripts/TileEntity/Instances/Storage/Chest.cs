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
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Chest")]
    public class Chest : TileEntity, IRightClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity, IConduitInteractable
    {
        [Header("Only input items")]
        ConduitPortLayout layout;
        [Tooltip("Rows of items")]
        public uint rows;
        [Tooltip("Columns of items")]
        public uint columns;
        [Tooltip("GUI Opened when clicked")]
        public GameObject gui;
        protected List<ItemSlot> items;

        public void onBreak()
        {
            if (items == null) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to spawn items in unloaded chunk");
                return;
            }
            foreach (ItemSlot itemSlot in items) {
                ItemEntityHelper.spawnItemEntityFromBreak(
                    getWorldPosition(),
                    itemSlot,
                    loadedChunk.getEntityContainer()
                );
            }
        }

        public void onRightClick()
        {
            if (gui == null) {
                Debug.LogError("GUI GameObject for chest:" + name + " null");
                return;
            }
            if (items == null) {
                initInventory();
            }
            GlobalUIController tileEntityGUIController = GlobalUIContainer.getInstance().getUiController();
            GameObject shownGui = GameObject.Instantiate(gui);
            SolidDynamicInventory inventoryGrid = shownGui.GetComponent<SolidDynamicInventory>();
            inventoryGrid.initalize(items, new Vector2Int((int) rows, (int) columns));
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
            return layout;
        }

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
            
        }

        private void initInventory() {
            items = new List<ItemSlot>();
            for (int i = 0; i < rows*columns;i++) {
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
