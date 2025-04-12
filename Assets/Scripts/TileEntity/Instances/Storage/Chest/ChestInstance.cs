using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Entities;
using Item.Slot;
using TileEntity.Instances.Storage.Chest;
using UI;

namespace TileEntity.Instances
{
    public interface IItemInventoryTileEntity : IItemConduitInteractable
    {
        public List<ItemSlot> Slots { get; }
        
    }

    public class SingleItemInventory : IItemConduitInteractable
    {
        public List<ItemSlot> Items;
        private int lastInsertSlot; // Always try to insert at first null slot

        public SingleItemInventory(List<ItemSlot> items)
        {
            Items = items;
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ItemSlot itemSlot = Items[i];
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                if (filter != null && !filter.Filter(itemSlot)) continue;
                return itemSlot;
            }

            return null;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (ItemSlotUtils.IsItemSlotNull(toInsert)) return;
            
            ItemSlot lastSlot = Items[lastInsertSlot];
            // Always try to insert into last insert slot first
            bool lastLastNull = ItemSlotUtils.IsItemSlotNull(lastSlot);
            if (!lastLastNull && ItemSlotUtils.CanInsertIntoSlot(lastSlot, toInsert, Global.MAX_SIZE))
            {
                ItemSlotUtils.InsertIntoSlot(lastSlot,toInsert, Global.MAX_SIZE);
                return;
            }
            
            const int NO_NULL_SLOT = -1;
            int firstNullIndex = lastLastNull ? lastInsertSlot : NO_NULL_SLOT;
          
            // Two loops to avoid try to insert into last insertion slot again
            for (int i = 0; i < lastInsertSlot; i++) {
                ItemSlot inputSlot = Items[i];
                if (ItemSlotUtils.IsItemSlotNull(inputSlot))
                {
                    if (firstNullIndex < 0) firstNullIndex = i;
                    continue;
                }
                if (!ItemSlotUtils.AreEqual(inputSlot,toInsert) || inputSlot.amount >= Global.MAX_SIZE) {
                    continue;
                }
                ItemSlotUtils.InsertIntoSlot(inputSlot,toInsert,Global.MAX_SIZE);
                lastInsertSlot = i;
                return;
            }
            
            for (int i = lastInsertSlot+1; i < Items.Count; i++) {
                ItemSlot inputSlot = Items[i];
                if (ItemSlotUtils.IsItemSlotNull(inputSlot))
                {
                    if (firstNullIndex < 0) firstNullIndex = i;
                    continue;
                }
                if (!ItemSlotUtils.AreEqual(inputSlot,toInsert) || inputSlot.amount >= Global.MAX_SIZE) {
                    continue;
                }
                ItemSlotUtils.InsertIntoSlot(inputSlot,toInsert,Global.MAX_SIZE);
                lastInsertSlot = i;
                return;
            }

            if (firstNullIndex == NO_NULL_SLOT) return;
            
            Items[firstNullIndex] = new ItemSlot(toInsert.itemObject,toInsert.amount,toInsert.tags);
            toInsert.amount=0;
            lastInsertSlot = firstNullIndex;
        }
    }
    public class ChestInstance : TileEntityInstance<Chest>, ISerializableTileEntity, IBreakActionTileEntity, 
        ISolidItemPortTileEntityAggregator, IBluePrintPlaceInitializedTileEntity, IConduitPortTileEntity, ISingleSolidInventoryTileEntity
    {
        public SingleItemInventory Inventory;
        public ChestInstance(Chest tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void OnBreak()
        {
            if (Inventory.Items == null) {
                return;
            }
            if (chunk is not ILoadedChunk loadedChunk) {
                Debug.LogError("Attempted to spawn items in unloaded chunk");
                return;
            }
            TileEntityUtils.spawnItemsOnBreak(Inventory.Items,GetWorldPosition(),loadedChunk);
        }
        
        public string Serialize()
        {
            return ItemSlotFactory.serializeList(Inventory.Items);
        }

        public void Unserialize(string data)
        {
            List<ItemSlot> items = ItemSlotFactory.Deserialize(data);
            Inventory = new SingleItemInventory(items);
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitLayout;
        }

        public IItemConduitInteractable GetSolidItemConduitInteractable()
        {
            return Inventory;
        }

        public void PlaceInitialize()
        {
            List<ItemSlot> items = new List<ItemSlot>();
            for (int i = 0; i < TileEntityObject.Rows*TileEntityObject.Columns;i++) {
                items.Add(null);
            }
            Inventory = new SingleItemInventory(items);
        }

        public List<ItemSlot> GetInventory()
        {
            return Inventory.Items;
        }
    }
}
