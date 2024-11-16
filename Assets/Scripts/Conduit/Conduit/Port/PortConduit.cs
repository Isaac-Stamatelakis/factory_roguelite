using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Conduits.Systems;
using Items;

namespace Conduits {
    public interface IPortConduit : IConduit {
        public IConduitPort getPort();
        public void setPort(IConduitPort port);
    }
    public abstract class PortConduit<TPort> : IPortConduit where TPort : IConduitPort
    {
        private int x;
        private int y;
        private ConduitItem conduitItem;
        private TPort port;
        private IConduitSystem conduitSystem;
        protected PortConduit(int x, int y,  ConduitItem conduitItem, TPort port) {
            this.x = x;
            this.y = y;
            this.conduitItem = conduitItem;
            this.port = port;
        }

        public ConduitItem GetConduitItem()
        {
            return conduitItem;
        }

        public IConduitPort getPort()
        {
            return port;
        }

        public string GetId()
        {
            return conduitItem.id;
        }
        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public void setPort(IConduitPort port)
        {
            this.port = (TPort) port;
        }

        public void SetConduitSystem(IConduitSystem conduitSystem)
        {
            this.conduitSystem = conduitSystem;
        }

        public IConduitSystem GetConduitSystem()
        {
            return this.conduitSystem;
        }

        public void SetX(int val)
        {
            x = val;
        }

        public void SetY(int val)
        {
            y = val;
        }
    }
}