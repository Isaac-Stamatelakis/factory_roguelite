using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {
    
    public class ItemConduitSystem : ConduitSystem<ItemConduitPort>
    {
        
        public ItemConduitSystem(string id) : base(id)
        {
            
        }
        private Dictionary<int, List<ItemConduitOutputPort>> coloredOutputPorts;
        private Dictionary<int, List<ItemConduitInputPort>> coloredPriorityInputs;

        public Dictionary<int, List<ItemConduitOutputPort>> ColoredOutputPorts { get => coloredOutputPorts; set => coloredOutputPorts = value; }
        public Dictionary<int, List<ItemConduitInputPort>> ColoredPriorityInputs { get => coloredPriorityInputs; set => coloredPriorityInputs = value; }

        public override void tickUpdate()
        {
            foreach (KeyValuePair<int,List<ItemConduitOutputPort>> colorOutputPortList in ColoredOutputPorts) {
                if (ColoredPriorityInputs.ContainsKey(colorOutputPortList.Key)) {
                    List<ItemConduitInputPort> priorityOrderInputs = ColoredPriorityInputs[colorOutputPortList.Key];
                    foreach (ItemConduitOutputPort itemConduitOutputPort in colorOutputPortList.Value) {
                        ItemSlot toInsert = itemConduitOutputPort.extract();
                        if (toInsert == null) {
                            continue;
                        }
                        int amount = Mathf.Min(toInsert.amount,itemConduitOutputPort.extractAmount);
                        ItemSlot tempItemSlot = new ItemSlot(itemObject: toInsert.itemObject, amount:amount,nbt: toInsert.nbt);
                        foreach (ItemConduitInputPort itemConduitInputPort in priorityOrderInputs) {
                            if (itemConduitInputPort.TileEntity == itemConduitOutputPort.TileEntity) {
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
                }
            }
        }

        private void addOutputPort(ItemConduitOutputPort outputPort) {
            if (outputPort == null) {
                return;
            }
            if (!ColoredOutputPorts.ContainsKey(outputPort.color)) {
                ColoredOutputPorts[outputPort.color] = new List<ItemConduitOutputPort>();
            }
            ColoredOutputPorts[outputPort.color].Add(outputPort);
        }
        private void addInputPort(ItemConduitInputPort inputPort) {
            if (inputPort == null) {
                return;
            }
            if (!ColoredPriorityInputs.ContainsKey(inputPort.color)) {
                ColoredPriorityInputs[inputPort.color] = new List<ItemConduitInputPort>();
            }
            List<ItemConduitInputPort> prioritySortedPorts = ColoredPriorityInputs[inputPort.color];
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<ItemConduitInputPort>.Create((p1, p2) => p2.priority.CompareTo(p1.priority)));
            if (index < 0) {
                // If negative, binary search couldn't find place, use bitwise complement
                index = ~index;
            }
            prioritySortedPorts.Insert(index, inputPort);
        }

        public override void merge(IConduitSystem otherConduitSystem)
        {
            base.merge(otherConduitSystem);
        }

        public override void addConduitToStructures(IConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not ItemConduitPort) {
                Debug.LogError("Item Conduit System recieved non item conduit port");
                return;
            }
            ItemConduitPort itemConduitPort = (ItemConduitPort) conduitPort;
            addInputPort(itemConduitPort.inputPort);
            addOutputPort(itemConduitPort.outputPort);
        }

        public override void rebuild()
        {
            ColoredOutputPorts = new Dictionary<int, List<ItemConduitOutputPort>>();
            ColoredPriorityInputs = new Dictionary<int, List<ItemConduitInputPort>>();
            foreach (ItemConduit itemConduit in conduits) {
                IConduitPort port = itemConduit.getPort();
                if (port == null) {
                    continue;
                }
                ItemConduitPort itemConduitPort = (ItemConduitPort) port;
                addOutputPort(itemConduitPort.outputPort);
                addInputPort(itemConduitPort.inputPort);
            }
        }

        protected override void init()
        {
            coloredOutputPorts = new Dictionary<int, List<ItemConduitOutputPort>>();
            coloredPriorityInputs = new Dictionary<int, List<ItemConduitInputPort>>();
        }
    }
}

