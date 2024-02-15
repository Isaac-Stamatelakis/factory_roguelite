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
        private List<ItemConduitInputPort> portsOrderByPriority;
        private Dictionary<int, List<ItemConduitOutputPort>> coloredOutputPorts;
        private Dictionary<int, List<ItemConduitInputPort>> coloredPriorityInputs;
        public override void tickUpdate()
        {
            foreach (KeyValuePair<int,List<ItemConduitOutputPort>> colorOutputPortList in coloredOutputPorts) {
                if (coloredPriorityInputs.ContainsKey(colorOutputPortList.Key)) {
                    List<ItemConduitInputPort> priorityOrderInputs = coloredPriorityInputs[colorOutputPortList.Key];
                    foreach (ItemConduitOutputPort itemConduitOutputPort in colorOutputPortList.Value) {
                        ItemSlot toInsert = itemConduitOutputPort.extract();
                        foreach (ItemConduitInputPort itemConduitInputPort in priorityOrderInputs) {
                            itemConduitInputPort.insert(toInsert);
                            if (toInsert.amount == 0) {
                                break;
                            } else if (toInsert.amount < 0) {
                                Debug.LogError("Something went wrong when inserting items. Got negative amount '" + toInsert.amount + "'");
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected override void generateSystem()
        {
            coloredOutputPorts = new Dictionary<int, List<ItemConduitOutputPort>>();
            coloredPriorityInputs = new Dictionary<int, List<ItemConduitInputPort>>();
            foreach (ItemConduitPort port in ports) {
                addOutputPort(port.outputPort);
                addInputPort(port.inputPort);
            }
        }

        private void addOutputPort(ItemConduitOutputPort outputPort) {
            if (outputPort == null) {
                return;
            }
            if (!coloredOutputPorts.ContainsKey(outputPort.color)) {
                coloredOutputPorts[outputPort.color] = new List<ItemConduitOutputPort>();
            }
            coloredOutputPorts[outputPort.color].Add(outputPort);
        }
        private void addInputPort(ItemConduitInputPort inputPort) {
            if (inputPort != null) {
                return;
            }
            if (!coloredPriorityInputs.ContainsKey(inputPort.color)) {
                coloredPriorityInputs[inputPort.color] = new List<ItemConduitInputPort>();
            }
            List<ItemConduitInputPort> prioritySortedPorts = new List<ItemConduitInputPort>();
            int index = prioritySortedPorts.BinarySearch(inputPort, Comparer<ItemConduitInputPort>.Create((p1, p2) => p2.priority.CompareTo(p1.priority)));
            prioritySortedPorts.Insert(index,inputPort); 
        }
    }
}

