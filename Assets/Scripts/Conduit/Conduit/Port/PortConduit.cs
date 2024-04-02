using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using ConduitModule.Systems;

namespace ConduitModule {
    public interface IPortConduit : IConduit {
        public IConduitPort getPort();
        public void setPort(IConduitPort port);
    }
    public abstract class PortConduit<Port> : IPortConduit where Port : IConduitPort
    {
        private int x;
        private int y;
        private ConduitItem conduitItem;
        private Port port;
        private IConduitSystem conduitSystem;
        public PortConduit(int x, int y,  ConduitItem conduitItem, Port port) {
            this.x = x;
            this.y = y;
            this.conduitItem = conduitItem;
            this.port = port;
        }

        public ConduitItem getConduitItem()
        {
            return conduitItem;
        }

        public IConduitPort getPort()
        {
            return port;
        }

        public string getId()
        {
            return conduitItem.id;
        }
        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }

        public void setPort(IConduitPort port)
        {
            this.port = (Port) port;
        }

        public void setConduitSystem(IConduitSystem conduitSystem)
        {
            this.conduitSystem = conduitSystem;
        }

        public IConduitSystem getConduitSystem()
        {
            return this.conduitSystem;
        }

        public void setX(int val)
        {
            x = val;
        }

        public void setY(int val)
        {
            y = val;
        }
    }
}