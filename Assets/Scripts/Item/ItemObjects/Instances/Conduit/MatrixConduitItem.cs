using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace Conduits {
    public class MatrixConduitItem : ConduitItem
    {
        public override ConduitType GetConduitType()
        {
            return ConduitType.Matrix;
        }
    }
}

