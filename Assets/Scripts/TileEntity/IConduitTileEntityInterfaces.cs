using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace TileEntityModule {
    public interface IConduitInteractable : ISoftLoadable {
        public ConduitPortLayout getConduitPortLayout();
    }
}

