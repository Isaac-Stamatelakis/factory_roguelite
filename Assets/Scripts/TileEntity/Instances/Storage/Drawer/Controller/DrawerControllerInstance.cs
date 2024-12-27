using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Storage {
    public class DrawerControllerInstance : TileEntityInstance<DrawerController>, ISolidItemConduitInteractable, IMultiBlockTileEntity
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

        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
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

        public ConduitPortLayout getConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public void InsertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (ItemSlotHelper.AreEqual(drawerSlot,itemSlot) && ItemSlotHelper.CanInsertIntoSlot(drawerSlot,itemSlot,drawer.Amount)) {
                    ItemSlotHelper.InsertIntoSlot(drawerSlot,itemSlot,drawerSlot.amount);
                }
                if (itemSlot.itemObject == null || itemSlot.amount == 0) {
                    return;
                }
            }
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (drawerSlot == null || drawerSlot.itemObject == null) {
                    drawerSlot = ItemSlotFactory.Copy(itemSlot);
                    itemSlot.itemObject = null;
                    return;
                }
            }
        }
    }
}

