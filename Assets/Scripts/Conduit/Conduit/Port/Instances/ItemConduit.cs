using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using Items;

namespace Conduits {
    public class ItemConduit : PortConduit<ItemTileEntityPort>
    {
        public ItemConduit(int x, int y,ConduitItem conduitItem, int state, ItemTileEntityPort port) : base(x, y, conduitItem, state,port)
        {
        }
    }
}

