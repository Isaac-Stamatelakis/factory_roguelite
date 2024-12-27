using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Chunks.Partitions;
using Conduits.Ports;
using System.Linq;

namespace Conduits.Systems {
    public interface IConduitSystem {
        public int GetSize();
        public bool ConnectsTo(IConduitSystem conduitSystem);
        public bool Contains(IConduit conduit);
        public void Merge(IConduitSystem conduitSystem);
        public void AddConduit(IConduit conduit);
        public string GetId();
        public HashSet<IConduit> GetConduits();
        public void Rebuild();
        public bool IsActive();
    }

    public interface IPortConduitSystem : IConduitSystem {
        public void TickUpdate();
    }

    public abstract class PortConduitSystem<TInPort, TOutPort> : ConduitSystem<IPortConduit>, IPortConduitSystem
    
        where TInPort : IColorPort 
        where TOutPort : IColorPort

    {
        private bool active;
        protected PortConduitSystem(string id,IConduitSystemManager manager) : base(id,manager) {
            init();
        }

        public override void AddConduit(IConduit conduit) {
            base.AddConduit(conduit);
            addPort((IPortConduit)conduit);
        }
        public override void Rebuild()
        {
            ColoredOutputPorts = new Dictionary<int, List<TOutPort>>();
            ColoredInputPorts = new Dictionary<int, List<TInPort>>();
            foreach (IPortConduit conduit in conduits) {
                addPort(conduit);
            }
        }

        public abstract void addPort(IPortConduit conduit);
        protected Dictionary<int, List<TOutPort>> coloredOutputPorts;
        protected Dictionary<int, List<TInPort>> coloredPriorityInputs;
        public Dictionary<int, List<TOutPort>> ColoredOutputPorts { get => coloredOutputPorts; set => coloredOutputPorts = value; }
        public Dictionary<int, List<TInPort>> ColoredInputPorts { get => coloredPriorityInputs; set => coloredPriorityInputs = value; }

        public abstract void TickUpdate();
        protected void addOutputPort(TOutPort outputPort) {
            if (outputPort == null) {
                return;
            }
            if (!ColoredOutputPorts.ContainsKey(outputPort.getColor())) {
                ColoredOutputPorts[outputPort.getColor()] = new List<TOutPort>();
            }
            ColoredOutputPorts[outputPort.getColor()].Add(outputPort);
        }
        protected void addInputPort(TInPort inputPort) {
            if (inputPort == null) {
                return;
            }
            if (!ColoredInputPorts.ContainsKey(inputPort.getColor())) {
                ColoredInputPorts[inputPort.getColor()] = new List<TInPort>();
            }
            ColoredInputPorts[inputPort.getColor()].Add(inputPort);
            addInputPortPostProcessing(inputPort);
        }

        protected abstract void addInputPortPostProcessing(TInPort inputPort);

        protected void init()
        {
            ColoredOutputPorts = new Dictionary<int, List<TOutPort>>();
            ColoredInputPorts = new Dictionary<int, List<TInPort>>();
        }

        protected void SetActive(bool state)
        {
            if (active == state)
            {
                return;
            }
            active = state;
            manager.RefreshSystemTiles(this);
        }

        public override bool IsActive()
        {
            return active;
        }
    }
    
}