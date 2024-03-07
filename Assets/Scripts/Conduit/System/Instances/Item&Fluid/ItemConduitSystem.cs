using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ConduitModule;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {

    public class SolidItemConduitSystem : ItemConduitSystem<ISolidItemConduitInteractable, ItemFilter>
    {
        public SolidItemConduitSystem(string id) : base(id)
        {
        }
    }
}

