using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using UnityEngine;

namespace TileEntity.Instances.Storage {
    public class DrawerControllerInstance : TileEntityInstance<DrawerController>, IItemConduitInteractable, IMultiBlockTileEntity, IConduitPortTileEntity, IRefreshOnItemExtractTileEntity
    {
        private List<ItemDrawerInstance> drawers;
        public DrawerControllerInstance(DrawerController tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void AssembleMultiBlock()
        {
            this.drawers = TileEntityUtils.BFSTileEntityComponent<ItemDrawerInstance>(this,TileType.Object);
            TileEntityUtils.SyncTileMultiBlockAggregates(this,this,this.drawers);
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot itemSlot = drawer.ItemSlot;
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                    continue;
                }
                
                return itemSlot;
            }
            return null;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (ItemSlotUtils.IsItemSlotNull(toInsert)) {
                return;
            }
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                uint maxSize = drawer.Amount;
                if (ItemSlotUtils.AreEqual(drawerSlot,toInsert) && ItemSlotUtils.CanInsertIntoSlot(drawerSlot,toInsert,maxSize)) {
                    ItemSlotUtils.InsertIntoSlot(drawerSlot,toInsert,maxSize);
                }
                if (ItemSlotUtils.IsItemSlotNull(toInsert)) {
                    return;
                }
            }
            foreach (ItemDrawerInstance drawer in drawers) {
                ItemSlot drawerSlot = drawer.ItemSlot;
                if (!ItemSlotUtils.IsItemSlotNull(drawerSlot)) continue;
                drawer.ItemSlot = ItemSlotFactory.Copy(toInsert);
                toInsert.amount = 0;
                drawer.LoadVisual();
                return;
            }
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitLayout;
        }

        public void RefreshOnExtraction()
        {
            
        }
    }
}

