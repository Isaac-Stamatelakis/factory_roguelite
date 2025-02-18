using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits;
using Conduits.Ports;
using Item.Slot;
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
            uint extractionRate = outputPort.GetExtractionRate(itemState);
            uint amount = toInsert.amount < extractionRate ? toInsert.amount : extractionRate;
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,tags: toInsert.tags);
            
            if (outputPort.GetOutputData().RoundRobin)
            {
                IterateRoundRobin(outputPort,inputPorts,tempItemSlot,toInsert,amount);
            }
            else
            {
                IterateStandard(outputPort,inputPorts,tempItemSlot,toInsert,amount);
            }
            outputPort.RefreshExtractor();
        }

        private void IterateStandard(ItemTileEntityPort outputPort, List<ItemTileEntityPort> inputPorts, ItemSlot tempItemSlot, ItemSlot toInsert, uint amount)
        {
            foreach (ItemTileEntityPort itemConduitInputPort in inputPorts) {
                if (ReferenceEquals(itemConduitInputPort.Interactable, outputPort.Interactable))continue;
                
                itemConduitInputPort.Insert(itemState, tempItemSlot);
                if (tempItemSlot.amount == 0) {
                    break;
                }
            }
            toInsert.amount -= amount-tempItemSlot.amount;
        }

        private void IterateRoundRobin(ItemTileEntityPort outputPort, List<ItemTileEntityPort> inputPorts, ItemSlot tempItemSlot, ItemSlot toInsert, uint amount)
        {
           
            if (inputPorts.Count == 1) // No need to bother with round-robin if only one input port
            {
                IterateStandard(outputPort,inputPorts,tempItemSlot,toInsert,amount);
                return;
            }
            var outputData = outputPort.GetOutputData();
            int start = outputData.RoundRobinIndex;
            int count = inputPorts.Count;
            int i = 0;
            while (i < count)
            {
                int index = (start + i) % count;
                i++;
                ItemTileEntityPort current = inputPorts[index];
                if (ReferenceEquals(current.Interactable, outputPort.Interactable)) continue;
                current.Insert(itemState, tempItemSlot);
                toInsert.amount -= amount-tempItemSlot.amount;
                bool inserted = tempItemSlot.amount != amount;
                if (inserted) break;
            }

            outputData.RoundRobinIndex = (start + i) % count;

        }
    }
    
}

