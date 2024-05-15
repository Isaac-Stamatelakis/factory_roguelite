using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;

namespace Conduits {
    public class ItemConduit : PortConduit<SolidItemConduitPort>
    {
        public ItemConduit(int x, int y,ConduitItem conduitItem, SolidItemConduitPort port) : base(x, y, conduitItem, port)
        {
        }
    }
}

