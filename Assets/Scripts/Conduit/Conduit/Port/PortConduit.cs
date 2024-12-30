using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Conduits.Systems;
using Items;

namespace Conduits {
    public interface IPortConduit : IConduit {
        public IConduitPort GetPort();
        public void SetPort(IConduitPort port);
    }

    public interface IConduitPort
    {
        
    }
    public abstract class PortConduit<TPort> : Conduit<ConduitItem>, IPortConduit where TPort : IConduitPort
    {
        private TPort port;
        protected PortConduit(int x, int y, ConduitItem conduitItem, int state, TPort port) : base(x, y, conduitItem, state)
        {
            this.port = port;
        }
        public IConduitPort GetPort()
        {
            return port;
        }

        public void SetPort(IConduitPort port)
        {
            this.port = (TPort)port;
        }
    }
}