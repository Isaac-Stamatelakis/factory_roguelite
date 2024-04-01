using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;
using ConduitModule.Ports;
using TileEntityModule;

namespace ConduitModule.Systems {
    
    public abstract class ItemConduitSystem<Port, InputPort, OutputPort> : PortConduitSystem<InputPort,OutputPort>
        where Port : IConduitPort
        where InputPort : IItemConduitInputPort
        where OutputPort : IItemConduitOutputPort
    {
        
        public ItemConduitSystem(string id) : base(id)
        {
            
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not Port port) {
                Debug.LogError("Item Conduit System recieved non item conduit port");
                return;
            }
            object input = port.getInputPort();
            if (input != null) {
                addInputPort((InputPort) port.getInputPort());
            } 
            object output = port.GetOutputPort();
            if (output != null) {
                addOutputPort((OutputPort) port.GetOutputPort());
            }
        }

        public override void iterateTickUpdate(OutputPort outputPort, List<InputPort> inputPorts)
        {
            ItemSlot toInsert = outputPort.extract();
            if (toInsert == null) {
                return;
            }
            int amount = Mathf.Min(toInsert.amount,outputPort.getExtractAmount());
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,tags: toInsert.tags);
            foreach (IItemConduitInputPort itemConduitInputPort in inputPorts) {
                if (itemConduitInputPort.getTileEntity().Equals(outputPort.getTileEntity())) {
                    continue;
                }
                itemConduitInputPort.insert(tempItemSlot);
                if (tempItemSlot.amount == 0) {
                    break;
                } else if (toInsert.amount < 0) {
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

        protected override void addInputPortPostProcessing(InputPort inputPort)
        {
            List<InputPort> prioritySortedPorts = ColoredInputPorts[inputPort.getColor()];
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<InputPort>.Create((p1, p2) => p2.getPriority().CompareTo(p1.getPriority())));
            if (index < 0) {
                // If negative, binary search couldn't find place, use bitwise complement
                index = ~index;
            }
            prioritySortedPorts.Insert(index, inputPort);
        }
    }
    
}

