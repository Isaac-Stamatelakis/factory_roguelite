using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.NodeNetwork {
    public interface INode
    {
        public Vector3 getPosition();
        public int getId();
        public HashSet<int> getPrerequisites();
    }
}

