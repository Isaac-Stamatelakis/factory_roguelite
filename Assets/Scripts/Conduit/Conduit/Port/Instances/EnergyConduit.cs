using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;

namespace Conduits {
    public class EnergyConduit : PortConduit<IConduitPort>
    {
        public EnergyConduit(int x, int y, ConduitItem conduitItem, IConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}
