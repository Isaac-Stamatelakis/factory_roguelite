using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;


namespace Conduits {
    public class SignalConduit : PortConduit<IConduitPort>
    {
        public SignalConduit(int x, int y, ConduitItem conduitItem, int state, IConduitPort port) : base(x, y, conduitItem, state,port)
        {
        }
    }
}
