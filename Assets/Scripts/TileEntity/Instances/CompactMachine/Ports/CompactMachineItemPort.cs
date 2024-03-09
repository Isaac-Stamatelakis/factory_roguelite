using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using ConduitModule.Ports;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.CompactMachines {
    public class CompactMachineItemPort : TileEntity, ISerializableTileEntity ,IConduitInteractable, IItemConduitInteractable, ICompactMachineInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        private ItemSlot itemSlot;
        private CompactMachine compactMachine;

        public ItemSlot extractItem()
        {
            return itemSlot;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public void insertItem(ItemSlot toInsert)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = toInsert;
                return;
            }
            ItemSlotHelper.handleInsert(itemSlot,toInsert);
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(ItemSlotFactory.seralizeItemSlot(itemSlot));
        }

        public void syncToCompactMachine(CompactMachine compactMachine)
        {
            this.compactMachine = compactMachine;
            compactMachine.Inventory.addPort(this,ConduitType.Energy);
        }

        public void unserialize(string data)
        {
            itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
        }
    }

}
