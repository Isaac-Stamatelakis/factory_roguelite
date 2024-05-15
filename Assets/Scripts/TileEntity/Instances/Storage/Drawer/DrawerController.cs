using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Drawer Controller",menuName="Tile Entity/Storage/Drawer/Controller")]
    public class DrawerController : TileEntity, ISolidItemConduitInteractable, IMultiBlockTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        private List<ItemDrawer> drawers;

        public void assembleMultiBlock()
        {
            List<ItemDrawer> drawers = new List<ItemDrawer>();
            TileEntityHelper.dfsTileEntity(this,drawers);
        }

        public ItemSlot extractSolidItem(Vector2Int portPosition)
        {
            foreach (ItemDrawer drawer in drawers) {
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
            return layout;
        }

        public void insertSolidItem(ItemSlot itemSlot, Vector2Int portPosition)
        {
            foreach (ItemDrawer drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (ItemSlotHelper.areEqual(drawerSlot,itemSlot) && ItemSlotHelper.canInsertIntoSlot(drawerSlot,itemSlot,drawer.Amount)) {
                    ItemSlotHelper.insertIntoSlot(drawerSlot,itemSlot,drawerSlot.amount);
                }
                if (itemSlot.itemObject == null || itemSlot.amount == 0) {
                    return;
                }
            }
            foreach (ItemDrawer drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (drawerSlot == null || drawerSlot.itemObject == null) {
                    drawerSlot = ItemSlotFactory.copy(itemSlot);
                    itemSlot.itemObject = null;
                    return;
                }
            }
        }
    }
}

