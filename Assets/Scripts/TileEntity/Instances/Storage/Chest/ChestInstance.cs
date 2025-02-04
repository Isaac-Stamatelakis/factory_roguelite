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
            ItemSlotUtils.InsertIntoInventory(Items, toInsert, Global.MaxSize);
        }
    }
    public class ChestInstance : TileEntityInstance<Chest>, IRightClickableTileEntity, ISerializableTileEntity, IBreakActionTileEntity, 
        IConduitPortTileEntityAggregator, IPlaceInitializable
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
            TileEntityUtils.spawnItemsOnBreak(Inventory.Items,getWorldPosition(),loadedChunk);
        }

        public void OnRightClick()
        {
            TileEntityObject.UIManager.Display<ChestInstance, ChestUI>(this,true);
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

        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Item:
                    return Inventory;
                case ConduitType.Fluid:
                case ConduitType.Energy:
                case ConduitType.Signal:
                case ConduitType.Matrix:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
        }

        public void PlaceInitialize()
        {
            List<ItemSlot> items = new List<ItemSlot>();
            for (int i = 0; i < TileEntityObject.Rows*TileEntityObject.Columns;i++) {
                items.Add(null);
            }
            Inventory = new SingleItemInventory(items);
        }
        
    }
}
