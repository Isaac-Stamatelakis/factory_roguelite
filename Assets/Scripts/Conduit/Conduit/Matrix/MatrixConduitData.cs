using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Conduits {
    [System.Serializable]
    public class MatrixConduitData 
    {
        public bool attached = false;
        public MatrixConduitData(bool attached) {
            this.attached = attached;
        }
    }
}

