using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GUIModule;
using ChunkModule;


namespace TileEntityModule.Instances
{
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Chest")]
    public class Chest : TileEntity, IClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity
    {
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

        public void onClick()
        {
            if (items == null) {
                items = new List<ItemSlot>();
                for (int i = 0; i < rows*columns;i++) {
                    items.Add(null);
                }
            }
            if (gui == null) {
                Debug.LogError("GUI GameObject for chest:" + name + " null");
                return;
            }
            GlobalUIController tileEntityGUIController = GlobalUIContainer.getInstance().getUiController();
            GameObject shownGui = GameObject.Instantiate(gui);
            DynamicInventoryGrid inventoryGrid = shownGui.GetComponent<DynamicInventoryGrid>();
            inventoryGrid.initalize(items, new Vector2Int((int) rows, (int) columns));
            tileEntityGUIController.setGUI(shownGui);
        }

        public string serialize()
        {
            return ItemSlotFactory.serializeList(items);
        }

        public void unserialize(string data)
        {
            this.items = ItemSlotFactory.deserialize(data);
        }
    }
}