using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits;
using Conduits.Ports;
using TileEntity;

namespace Conduits.Systems {
    
    public abstract class ItemConduitSystem<TPort, TInputPort, TOutputPort> : ResourcePortConduitSystem<TInputPort,TOutputPort>
        where TPort : IConduitPort
        where TInputPort : IItemConduitInputPort
        where TOutputPort : IItemConduitOutputPort
    {
        
        public ItemConduitSystem(string id, IConduitSystemManager manager) : base(id, manager)
        {
            
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.GetPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not TPort port) {
                Debug.LogError("Item Conduit System recieved non item conduit port");
                return;
            }
            object input = port.getInputPort();
            if (input != null) {
                addInputPort((TInputPort) port.getInputPort());
            } 
            object output = port.GetOutputPort();
            if (output != null) {
                addOutputPort((TOutputPort) port.GetOutputPort());
            }
        }
        
        protected override void IterateTickUpdate(TOutputPort outputPort, List<TInputPort> inputPorts, int color)
        {
            ItemSlot toInsert = outputPort.Extract();
            if (toInsert == null) {
                return;
            }

            activeThisTick = true;
            uint amount = toInsert.amount < outputPort.GetExtractAmount() ? toInsert.amount : outputPort.GetExtractAmount();
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,tags: toInsert.tags);
            foreach (IItemConduitInputPort itemConduitInputPort in inputPorts) {
                if (itemConduitInputPort.GetConduitInteractable().Equals(outputPort.GetConduitInteractable())) {
                    continue;
                }
                itemConduitInputPort.Insert(tempItemSlot);
                if (tempItemSlot.amount == 0) {
                    break;
                }
                if (toInsert.amount < 0) {
                    Debug.LogError("Something went wrong when inserting items. Got negative amount '" + tempItemSlot.amount + "'");
                    break;
                }
            }
            toInsert.amount -= amount-tempItemSlot.amount;
            if (toInsert.amount <= 0) {
                toInsert.itemObject = null;
                if (toInsert.amount < 0) {
                    Debug.LogError("Negative amount something went wrong inserting item conduit system");
                }
            }
        }

        protected override void addInputPortPostProcessing(TInputPort inputPort)
        {
            List<TInputPort> prioritySortedPorts = ColoredInputPorts[inputPort.getColor()];
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<TInputPort>.Create((p1, p2) => p2.getPriority().CompareTo(p1.getPriority())));
            if (index < 0) {
                // If negative, binary search couldn't find place, use bitwise complement
                index = ~index;
            }
            prioritySortedPorts.Insert(index, inputPort);
        }
    }
    
}

