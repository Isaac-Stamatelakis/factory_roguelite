using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {

    public class FluidConduitSystem : ItemConduitSystem<IFluidConduitInteractable, FluidFilter>
    {
        public FluidConduitSystem(string id) : base(id)
        {
        }
    }
}

