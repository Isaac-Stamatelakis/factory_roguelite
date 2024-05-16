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
        private IMatrixConduitInteractable matrixConduitInteractable;
        public bool HasTileEntity {get => matrixConduitInteractable != null;}
        public IMatrixConduitInteractable MatrixConduitInteractable { get => matrixConduitInteractable; set => matrixConduitInteractable = value; }

        public MatrixConduit(int x, int y, MatrixConduitItem item, IMatrixConduitInteractable matrixConduitInteractable) {
            this.x = x;
            this.y = y;
            this.matrixConduitItem = item;
            this.matrixConduitInteractable = matrixConduitInteractable;
        }
        public ConduitItem getConduitItem()
        {
            return matrixConduitItem;
        }

        public IConduitSystem getConduitSystem()
        {
            return conduitSystem;
        }

        public string getId()
        {
            return matrixConduitItem.id;
        }


        public int getX()
        {
            return x;
        }

        public int getY()
        {
            return y;
        }

        public void setConduitSystem(IConduitSystem conduitSystem)
        {
            this.conduitSystem = conduitSystem;
        }


        public void setX(int val)
        {
            this.x = val;
        }

        public void setY(int val)
        {
            this.y = val;
        }
    }
}

