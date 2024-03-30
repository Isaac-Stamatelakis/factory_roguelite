using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;

namespace ConduitModule.Systems {
    public class MatrixConduitSystem : ConduitSystem<IConduit>
    {
        private List<IMatrixConduitInteractable> tileEntities;
        public MatrixConduitSystem(string id) : base(id)
        {
            
        }

        public override void rebuild()
        {
            tileEntities = new List<IMatrixConduitInteractable>();
        }

        public override void tickUpdate()
        {
            
        }
    }
}

