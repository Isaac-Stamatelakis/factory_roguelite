using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ChunkModule.PartitionModule;
using ConduitModule.Ports;
using System.Linq;

namespace ConduitModule.Systems {
    public interface IConduitSystem {
        public int getSize();
        public bool connectsTo(IConduitSystem conduitSystem);
        public bool contains(IConduit conduit);
        public void merge(IConduitSystem conduitSystem);
        public void addConduit(IConduit conduit);
        public string getId();
        public HashSet<IConduit> getConduits();
        public void rebuild();
    }

    public interface IPortConduitSystem : IConduitSystem {
        public void tickUpdate();
    }

    public abstract class PortConduitSystem<InPort, OutPort> : ConduitSystem<IPortConduit>, IPortConduitSystem
    
        where InPort : IColorPort 
        where OutPort : IColorPort
        
        {
        public PortConduitSystem(string id) : base(id) {
            init();
        }


        public override void addConduit(IConduit conduit) {
            base.addConduit(conduit);
            addPort((IPortConduit)conduit);
        }
        public override void rebuild()
        {
            ColoredOutputPorts = new Dictionary<int, List<OutPort>>();
            ColoredInputPorts = new Dictionary<int, List<InPort>>();
            foreach (IPortConduit conduit in conduits) {
                addPort(conduit);
            }
        }

        public abstract void addPort(IPortConduit conduit);
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