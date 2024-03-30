using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule {
    public class MatrixConduitItem : ConduitItem
    {
        public override ConduitType getType()
        {
            return ConduitType.Matrix;
        }
    }
}

