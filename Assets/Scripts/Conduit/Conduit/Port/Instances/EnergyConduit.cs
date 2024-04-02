using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule {
    public class EnergyConduit : PortConduit<IConduitPort>
    {
        public EnergyConduit(int x, int y, ConduitItem conduitItem, IConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}
