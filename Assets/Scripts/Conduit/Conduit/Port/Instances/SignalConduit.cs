using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;


namespace ConduitModule {
    public class SignalConduit : PortConduit<IConduitPort>
    {
        public SignalConduit(int x, int y, ConduitItem conduitItem, IConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}
