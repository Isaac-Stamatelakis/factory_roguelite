using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using ConduitModule.Ports;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Item Port", menuName = "Tile Entity/Compact Machine/Port/Item")]
    public class CompactMachineItemPort : TileEntity, ISerializableTileEntity ,IConduitInteractable, ISolidItemConduitInteractable, ICompactMachineInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        private ItemSlot itemSlot;
        private CompactMachine compactMachine;

        public ItemSlot extractItem(Vector2Int portPosition)
        {
            return itemSlot;
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public void insertItem(ItemSlot toInsert,Vector2Int portPosition)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                itemSlot = ItemSlotFactory.copy(toInsert);
                toInsert.amount=0;
                return;
            }
        }

        public string serialize()
        {
            return ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public void syncToCompactMachine(CompactMachine compactMachine)
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
