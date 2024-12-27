using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.CompactMachines {
    public class CompactMachineItemPortInstance : TileEntityInstance<CompactMachineItemPort>, ISerializableTileEntity ,IConduitTileEntity, ISolidItemConduitInteractable, ICompactMachineInteractable
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

        public void InsertSolidItem(ItemSlot toInsert,Vector2Int portPosition)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.Copy(toInsert);
                toInsert.amount=0;
                return;
            }
        }

        public string serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void syncToCompactMachine(CompactMachineInstance compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Inventory.addPort(this,ConduitType.Item);
        }

        public void unserialize(string data)
        {
            itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
        }
    }

}
