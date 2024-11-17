using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Items;

namespace Conduits {
    public class MatrixConduit : Conduit<MatrixConduitItem>
    {
        private int x;
        private int y;
        private MatrixConduitItem matrixConduitItem;
        private IConduitSystem conduitSystem;
        public bool HasTileEntity => MatrixConduitInteractable != null;
        public IMatrixConduitInteractable MatrixConduitInteractable { get; set; }

        public MatrixConduit(int x, int y, MatrixConduitItem item, int state, IMatrixConduitInteractable matrixConduitInteractable) : base(x,y,item, state)
        {
            this.x = x;
            this.y = y;
            this.matrixConduitItem = item;
            this.MatrixConduitInteractable = matrixConduitInteractable;
        }
    }
}

