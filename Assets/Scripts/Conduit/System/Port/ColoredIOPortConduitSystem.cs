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

    public enum PortConnectionType
    {
        Input,
        Output
    }
    public interface IColoredTileEntityPort
    {
        public int GetColor(PortConnectionType portConnectionType);
        public int SetColor(PortConnectionType portConnectionType, int color);
    }

    public abstract class ColoredIOPortConduitSystem<TTileEntityPort> : ConduitSystem<IPortConduit>, IPortConduitSystem where TTileEntityPort : IColoredTileEntityPort, IOConduitPort

    {
        protected Dictionary<int, List<TTileEntityPort>> coloredOutputPorts;
        protected Dictionary<int, List<TTileEntityPort>> coloredPriorityInputs;
        protected int activeDisplayTicks;
        protected ColoredIOPortConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
        {
            coloredOutputPorts = new Dictionary<int, List<TTileEntityPort>>();
            coloredPriorityInputs = new Dictionary<int, List<TTileEntityPort>>();
        }

        public override void AddConduit(IConduit conduit) {
            base.AddConduit(conduit);
            AddPort((IPortConduit)conduit);
        }
        public override void Rebuild()
        {
            coloredOutputPorts = new Dictionary<int, List<TTileEntityPort>>();
            coloredPriorityInputs = new Dictionary<int, List<TTileEntityPort>>();
            foreach (IPortConduit conduit in conduits) {
                AddPort(conduit);
            }
        }

        private void AddPort(IPortConduit conduit)
        {
            if (conduit.GetPort() is not TTileEntityPort tileEntityPort) return;
            AddInputPort(tileEntityPort);
            AddOutputPort(tileEntityPort);
        }

        public void TickUpdate()
        {
            activeDisplayTicks--;
            if (activeDisplayTicks == 0)
            {
                manager.RefreshSystemTiles(this);
            }
            SystemTickUpdate();
        }

        public abstract void SystemTickUpdate();
        protected void AddOutputPort(TTileEntityPort tileEntityPort)
        {
            if (ReferenceEquals(tileEntityPort?.GetPortData(PortConnectionType.Output),null)) return;
            if (!tileEntityPort.GetPortData(PortConnectionType.Output).Enabled) return;
            int color = tileEntityPort.GetColor(PortConnectionType.Output);
            if (!coloredOutputPorts.ContainsKey(color))
            {
                coloredOutputPorts[color] = new List<TTileEntityPort>();
            }
            coloredOutputPorts[color].Add(tileEntityPort);
        }
        protected void AddInputPort(TTileEntityPort tileEntityPort) {
            if (ReferenceEquals(tileEntityPort?.GetPortData(PortConnectionType.Input),null)) return;
            if (!tileEntityPort.GetPortData(PortConnectionType.Input).Enabled) return;
            
            bool loaded = manager.IsSystemLoaded();
            if (!loaded && tileEntityPort.GetInteractable() is ISystemLoadedConduitPortTileEntity) return;
            int color = tileEntityPort.GetColor(PortConnectionType.Input);
            if (!coloredPriorityInputs.ContainsKey(color)) {
                coloredPriorityInputs[color] = new List<TTileEntityPort>();
            }
            coloredPriorityInputs[color].Add(tileEntityPort);
            AddInputPortPostProcessing(tileEntityPort);
        }

        protected abstract void AddInputPortPostProcessing(TTileEntityPort inputPort);
        protected abstract void SetActive(bool state);

        public override bool IsActive()
        {
            return activeDisplayTicks > 0;
        }
    }
    
}