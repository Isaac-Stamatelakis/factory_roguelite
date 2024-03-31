using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;
using ConduitModule.Ports;

namespace ConduitModule.Systems {

    public class SolidItemConduitSystem : ItemConduitSystem<SolidItemConduitPort, SolidItemConduitInputPort, SolidItemConduitOutputPort>
    {
        public SolidItemConduitSystem(string id) : base(id)
        {
        }
    }
}

