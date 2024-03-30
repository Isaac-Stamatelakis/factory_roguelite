using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;

namespace ConduitModule.Systems {
    public class MatrixConduitSystem : ConduitSystem<IConduit>
    {
        private List<MatrixInterface> interfaces;
        private ItemMatrixController controller;
        public MatrixConduitSystem(string id) : base(id)
        {
            
        }

        public override void rebuild()
        {
            interfaces = new List<MatrixInterface>();
            controller = null;
        }

        public override void tickUpdate()
        {
            if (controller == null) {
                return;
            }
        }
    }
}

