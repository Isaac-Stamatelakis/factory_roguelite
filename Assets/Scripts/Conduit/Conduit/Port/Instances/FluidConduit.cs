using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;

namespace Conduits {
    public class FluidConduit : PortConduit<FluidItemConduitPort>
    {
        public FluidConduit(int x, int y, ConduitItem conduitItem, int state, FluidItemConduitPort port) : base(x, y, conduitItem, state,port)
        {
        }
    }
}