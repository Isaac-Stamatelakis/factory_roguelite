using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;
using Items;

namespace Conduits {
    public class MatrixConduit : IConduit
    {
        private int x;
        private int y;
        private MatrixConduitItem matrixConduitItem;
        private IConduitSystem conduitSystem;
        public bool HasTileEntity => MatrixConduitInteractable != null;
        public IMatrixConduitInteractable MatrixConduitInteractable { get; set; }

        public MatrixConduit(int x, int y, MatrixConduitItem item, IMatrixConduitInteractable matrixConduitInteractable) {
            this.x = x;
            this.y = y;
            this.matrixConduitItem = item;
            this.MatrixConduitInteractable = matrixConduitInteractable;
        }
        public ConduitItem GetConduitItem()
        {
            return matrixConduitItem;
        }

        public IConduitSystem GetConduitSystem()
        {
            return conduitSystem;
        }

        public string GetId()
        {
            return matrixConduitItem.id;
        }


        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }

        public void SetConduitSystem(IConduitSystem newSystem)
        {
            this.conduitSystem = newSystem;
        }


        public void SetX(int val)
        {
            this.x = val;
        }

        public void SetY(int val)
        {
            this.y = val;
        }
    }
}

