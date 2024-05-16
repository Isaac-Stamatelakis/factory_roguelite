using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {

    public class FluidConduitSystem : ItemConduitSystem<FluidItemConduitPort, FluidItemConduitInputPort, FluidItemConduitOutputPort>
    {
        public FluidConduitSystem(string id) : base(id)
        {
        }
    }
}

