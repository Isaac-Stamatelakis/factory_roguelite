using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;
using Item.Slot;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineItemPortInstance : CompactMachinePortInstance<CompactMachineItemPort>, ISerializableTileEntity, IItemConduitInteractable
    {
        public CompactMachineItemPortInstance(CompactMachineItemPort tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private ItemSlot itemSlot;
        private CompactMachineInstance compactMachine;
        
        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }
        

        public void Unserialize(string data)
        {
            itemSlot = ItemSlotFactory.DeserializeSlot(data);
        }

        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter)
        {
            return itemSlot;
        }

        public void InsertItem(ItemState state, ItemSlot toInsert, Vector2Int portPosition)
        {
            if (!ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            itemSlot = new ItemSlot(toInsert.itemObject, toInsert.amount, toInsert.tags);
            toInsert.amount=0;
        }

        public override ConduitType GetConduitType()
        {
            return ConduitType.Item;
        }
    }

}
