using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.NodeNetwork {
    public interface INodeNetwork<T> {
        public List<T> getNodes();
    }
}

