using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {
    
    public abstract class ItemConduitSystem<Interactable, Filter> : ConduitSystem<ItemConduitInputPort<Interactable,Filter>, ItemConduitOutputPort<Interactable,Filter>>
        where Interactable : IItemConduitInteractable
        where Filter : IFilter
    {
        
        public ItemConduitSystem(string id) : base(id)
        {
            
        }

        public override void addPort(IConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not AbstractItemConduitPort<Interactable,Filter> itemPort) {
                Debug.LogError("Item Conduit System recieved non item conduit port");
                return;
            }
            object input = itemPort.getInputPort();
            if (input != null) {
                addInputPort((ItemConduitInputPort<Interactable,Filter>) itemPort.getInputPort());
            } 
            object output = itemPort.GetOutputPort();
            if (output != null) {
                addOutputPort((ItemConduitOutputPort<Interactable,Filter>) itemPort.GetOutputPort());
            }
        }

        public override void iterateTickUpdate(ItemConduitOutputPort<Interactable, Filter> outputPort, List<ItemConduitInputPort<Interactable, Filter>> inputPorts)
        {
            ItemSlot toInsert = outputPort.extract();
            if (toInsert == null) {
                return;
            }
            int amount = Mathf.Min(toInsert.amount,outputPort.extractAmount);
            ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,tags: toInsert.tags);
            foreach (ItemConduitInputPort<Interactable,Filter> itemConduitInputPort in inputPorts) {
                if (itemConduitInputPort.TileEntity.Equals(outputPort.TileEntity)) {
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

        protected override void addInputPortPostProcessing(ItemConduitInputPort<Interactable,Filter> inputPort)
        {
            List<ItemConduitInputPort<Interactable,Filter>> prioritySortedPorts = ColoredInputPorts[inputPort.color];
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<ItemConduitInputPort<Interactable,Filter>>.Create((p1, p2) => p2.priority.CompareTo(p1.priority)));
            if (index < 0) {
                // If negative, binary search couldn't find place, use bitwise complement
                index = ~index;
            }
            prioritySortedPorts.Insert(index, inputPort);
        }
    }
    
}

