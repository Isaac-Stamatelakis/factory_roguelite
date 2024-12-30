using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Storage {
    public class DrawerControllerInstance : TileEntityInstance<DrawerController>, IItemConduitInteractable, IMultiBlockTileEntity
    {
        private List<ItemDrawerInstance> drawers;
        public DrawerControllerInstance(DrawerController tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void assembleMultiBlock()
        {
            List<ItemDrawerInstance> drawers = new List<ItemDrawerInstance>();
            TileEntityHelper.dfsTileEntity(this,drawers);
        }
        
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot itemSlot = drawer.ItemSlot;
                if (itemSlot == null || itemSlot.itemObject == null) {
                    continue;
                }
                return itemSlot;
            }
            return null;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (ItemSlotHelper.AreEqual(drawerSlot,toInsert) && ItemSlotHelper.CanInsertIntoSlot(drawerSlot,toInsert,drawer.Amount)) {
                    ItemSlotHelper.InsertIntoSlot(drawerSlot,toInsert,drawerSlot.amount);
                }
                if (toInsert.itemObject == null || toInsert.amount == 0) {
                    return;
                }
            }
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (drawerSlot == null || drawerSlot.itemObject == null) {
                    drawerSlot = ItemSlotFactory.Copy(toInsert);
                    toInsert.itemObject = null;
                    return;
                }
            }
        }
    }
}

