using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule.PartitionModule;
using ConduitModule.Ports;
using System.Linq;

namespace ConduitModule.ConduitSystemModule {
    public interface IConduitSystem {
        public void tickUpdate();
        public int getSize();
        public bool connectsTo(IConduitSystem conduitSystem);
        public bool contains(IConduit conduit);
        public void merge(IConduitSystem conduitSystem);
        public void addConduit(IConduit conduit);
        public string getId();
        public HashSet<IConduit> getConduits();
        public void rebuild();
    }
    public abstract class ConduitSystem<InPort, OutPort> : IConduitSystem 
    
        where InPort : IColorPort 
        where OutPort : IColorPort
        
        {
        public ConduitSystem(string id) {
            this.id = id;
            Conduits = new HashSet<IConduit>();
            init();
        }
        protected HashSet<IConduit> conduits;
        private string id;
        protected HashSet<IConduit> Conduits { get => conduits; set => conduits = value; }

        public void addConduit(IConduit conduit) {
            Conduits.Add(conduit);
            conduit.setConduitSystem(this);
            addPort(conduit);
        }

        public bool connectsTo(IConduitSystem otherConduitSystem) {
            // If otherConduitSystem is smaller, check it instead
            if (otherConduitSystem.getSize() < Conduits.Count) {
                return otherConduitSystem.connectsTo(this);
            }
            // Check otherConduitSystem contains any conduit in this one. O(n)
            foreach (IConduit conduit in Conduits) {
                if (otherConduitSystem.contains(conduit)) {
                    return true;
                }
            }
            return false;
        }
        public virtual void merge(IConduitSystem otherConduitSystem) {
            foreach (IConduit conduit in otherConduitSystem.getConduits()) {
                addConduit(conduit);
                conduit.setConduitSystem(this);
            }
        }

        public int getSize()
        {
            return Conduits.Count;
        }

        public bool contains(IConduit conduit)
        {
            return Conduits.Contains(conduit);
        }

        public string getId()
        {
            return id;
        }

        public HashSet<IConduit> getConduits()
        {
            return conduits;
        }

        public void rebuild()
        {
            ColoredOutputPorts = new Dictionary<int, List<OutPort>>();
            ColoredInputPorts = new Dictionary<int, List<InPort>>();
            foreach (IConduit conduit in conduits) {
                addPort(conduit);
            }
        }

        public abstract void addPort(IConduit conduit);
        private Dictionary<int, List<OutPort>> coloredOutputPorts;
        private Dictionary<int, List<InPort>> coloredPriorityInputs;

        public Dictionary<int, List<OutPort>> ColoredOutputPorts { get => coloredOutputPorts; set => coloredOutputPorts = value; }
        public Dictionary<int, List<InPort>> ColoredInputPorts { get => coloredPriorityInputs; set => coloredPriorityInputs = value; }

        public void tickUpdate()
        {
            foreach (KeyValuePair<int,List<OutPort>> colorOutputPortList in ColoredOutputPorts) {
                if (ColoredInputPorts.ContainsKey(colorOutputPortList.Key)) {
                    List<InPort> priorityOrderInputs = ColoredInputPorts[colorOutputPortList.Key];
                    foreach (OutPort itemConduitOutputPort in colorOutputPortList.Value) {
                        iterateTickUpdate(itemConduitOutputPort,priorityOrderInputs);
                    }
                }
            }
        }

        public abstract void iterateTickUpdate(OutPort outputPort, List<InPort> inputPort);

        protected void addOutputPort(OutPort outputPort) {
            if (outputPort == null) {
                return;
            }
            if (!ColoredOutputPorts.ContainsKey(outputPort.getColor())) {
                ColoredOutputPorts[outputPort.getColor()] = new List<OutPort>();
            }
            ColoredOutputPorts[outputPort.getColor()].Add(outputPort);
        }
        protected void addInputPort(InPort inputPort) {
            if (inputPort == null) {
                return;
            }
            if (!ColoredInputPorts.ContainsKey(inputPort.getColor())) {
                ColoredInputPorts[inputPort.getColor()] = new List<InPort>();
            }
            ColoredInputPorts[inputPort.getColor()].Add(inputPort);
            addInputPortPostProcessing(inputPort);
        }

        protected abstract void addInputPortPostProcessing(InPort inputPort);

        protected void init()
        {
            ColoredOutputPorts = new Dictionary<int, List<OutPort>>();
            ColoredInputPorts = new Dictionary<int, List<InPort>>();
        }
    }
}