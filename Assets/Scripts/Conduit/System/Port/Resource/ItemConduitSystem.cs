using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits;
using Conduits.Ports;
using TileEntity;

namespace Conduits.Systems {
    
    public class ItemConduitSystem : ResourceColoredIOPortConduitSystem<ItemTileEntityPort>
    {
        private ItemState itemState;
        public ItemConduitSystem(string id, IConduitSystemManager manager, ItemState itemState) : base(id, manager)
        {
            this.itemState = itemState;
        }
        
        protected override void AddInputPortPostProcessing(ItemTileEntityPort inputPort)
        {
            var prioritySortedPorts = coloredPriorityInputs[inputPort.GetColor(PortConnectionType.Input)];
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<ItemTileEntityPort>.Create(
                (p1, p2) => p2.GetInputData().Priority.CompareTo(p1.GetInputData().Priority)
            ));
            if (index < 0) {
                // If negative, binary search couldn't find place, use bitwise complement
                index = ~index;
            }
            prioritySortedPorts.Insert(index, inputPort);
        }
        
        protected override void IterateTickUpdate(ItemTileEntityPort outputPort, List<ItemTileEntityPort> inputPorts, int color)
        {
            ItemSlot toInsert = outputPort.Extract(itemState);
            if (toInsert == null) {
                return;
            }

            activeThisTick = true;
            uint extractionRate = outputPort.GetExtractionRate();
            uint amount = toInsert.amount < extractionRate ? toInsert.amount : extractionRate;
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,tags: toInsert.tags);
            foreach (ItemTileEntityPort itemConduitInputPort in inputPorts) {
                if (itemConduitInputPort.Interactable.Equals(outputPort.Interactable)) {
                    continue;
                }
                itemConduitInputPort.Insert(itemState, tempItemSlot);
                if (tempItemSlot.amount == 0) {
                    break;
                }
            }
            toInsert.amount -= amount-tempItemSlot.amount;
        }
    }
    
}

