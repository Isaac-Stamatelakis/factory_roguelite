using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace ConduitModule {
    public class MatrixConduit : IConduit
    {
        private int x;
        private int y;
        private MatrixConduitItem matrixConduitItem;
        private IConduitSystem conduitSystem;
        public MatrixConduit(int x, int y, MatrixConduitItem item) {
            this.x = x;
            this.y = y;
            this.matrixConduitItem = item;
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

