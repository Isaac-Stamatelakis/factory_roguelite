using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;
using Item.Slot;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineItemPortInstance : TileEntityInstance<CompactMachineItemPort>, ISerializableTileEntity ,IConduitPortTileEntity, IItemConduitInteractable, ICompactMachineInteractable
    {
        public CompactMachineItemPortInstance(CompactMachineItemPort tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        private ItemSlot itemSlot;
        private CompactMachineInstance compactMachine;
        public ItemSlot ExtractSolidItem(Vector2Int portPosition)
        {
            return itemSlot;
        }

        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.Layout;
        }
        
        public string Serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Inventory.addPort(this,ConduitType.Item);
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
            if (ReferenceEquals(toInsert?.itemObject, null)) return;
            toInsert = ItemSlotFactory.Copy(toInsert);
            toInsert.amount=0;
        }
    }

}
