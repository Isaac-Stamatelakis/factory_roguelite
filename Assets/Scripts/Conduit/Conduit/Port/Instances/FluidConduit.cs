using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;

namespace Conduits {
    public class FluidConduit : PortConduit<FluidItemConduitPort>
    {
        public FluidConduit(int x, int y, ConduitItem conduitItem, FluidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}