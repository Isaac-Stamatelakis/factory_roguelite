using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule.Systems {

    public class FluidConduitSystem : ItemConduitSystem<FluidItemConduitPort, FluidItemConduitInputPort, FluidItemConduitOutputPort>
    {
        public FluidConduitSystem(string id) : base(id)
        {
        }
    }
}
