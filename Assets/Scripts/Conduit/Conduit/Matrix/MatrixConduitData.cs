using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Conduits {
    [System.Serializable]
    public class MatrixConduitData : ConduitData
    {
        public bool attached = false;
        public MatrixConduitData(int state, bool attached) : base(state)
        {
            this.attached = attached;
        }
    }
}

