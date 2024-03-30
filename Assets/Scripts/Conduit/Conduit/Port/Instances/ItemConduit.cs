using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule {
    public class ItemConduit : PortConduit<SolidItemConduitPort>
    {
        public ItemConduit(int x, int y,ConduitItem conduitItem, SolidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}

