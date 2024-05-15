using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace TileEntityModule {
    public interface IConduitInteractable : ISoftLoadable {
        public ConduitPortLayout getConduitPortLayout();
    }
}

