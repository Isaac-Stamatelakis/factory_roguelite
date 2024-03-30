using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule {
    public class FluidConduit : PortConduit<FluidItemConduitPort>
    {
        public FluidConduit(int x, int y, ConduitItem conduitItem, FluidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}