using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.PartitionModule;
using TileMapModule.Layer;
using ConduitModule.Ports;
using ConduitModule.ConduitSystemModule;

namespace ConduitModule {
    public interface IConduit {
        
        public int getX();
        public int getY();
        public void setX(int val);
        public void setY(int val);
        public ConduitItem getConduitItem();
        public IConduitPort getPort();
        public void setPort(IConduitPort port);
        public string getId();
        public void setConduitSystem(IConduitSystem conduitSystem);
        public IConduitSystem getConduitSystem();

    }
    public abstract class Conduit<Port> : IConduit where Port : IConduitPort
    {
        private int x;
        private int y;
        private ConduitItem conduitItem;
        private Port port;
        private IConduitSystem conduitSystem;
        public Conduit(int x, int y,  ConduitItem conduitItem, Port port) {
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

    public class ItemConduit : Conduit<SolidItemConduitPort>
    {
        public ItemConduit(int x, int y,ConduitItem conduitItem, SolidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }

    public class FluidConduit : Conduit<FluidItemConduitPort>
    {
        public FluidConduit(int x, int y, ConduitItem conduitItem, FluidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }

    public class SignalConduit : Conduit<IConduitPort>
    {
        public SignalConduit(int x, int y, ConduitItem conduitItem, IConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
    public class EnergyConduit : Conduit<IConduitPort>
    {
        public EnergyConduit(int x, int y, ConduitItem conduitItem, IConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
   
}
