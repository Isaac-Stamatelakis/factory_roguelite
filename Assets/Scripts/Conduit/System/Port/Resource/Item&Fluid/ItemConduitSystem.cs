using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Conduits;
using Conduits.Ports;

namespace Conduits.Systems {

    public class SolidItemConduitSystem : ItemConduitSystem<SolidItemConduitPort, SolidItemConduitInputPort, SolidItemConduitOutputPort>
    {
        public SolidItemConduitSystem(string id, IConduitSystemManager manager) : base(id, manager)
        {
        }
    }
}

