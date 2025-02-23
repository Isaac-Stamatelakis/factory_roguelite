using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.NodeNetwork {
    public interface INode
    {
        public Vector3 GetPosition();
        public void SetPosition(Vector3 pos);
        public int GetId();
        public List<int> GetPrerequisites();
        public bool IsCompleted();
    }
}

