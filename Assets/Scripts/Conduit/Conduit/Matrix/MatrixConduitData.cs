using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ConduitModule {
    [System.Serializable]
    public class MatrixConduitData 
    {
        public bool attached = false;
        public MatrixConduitData(bool attached) {
            this.attached = attached;
        }
    }
}

