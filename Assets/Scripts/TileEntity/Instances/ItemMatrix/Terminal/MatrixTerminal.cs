using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminal : TileEntity, IMatrixConduitInteractable, IRightClickableTileEntity
    {
        private ItemMatrixController controller;
        public ConduitPortLayout getConduitPortLayout()
        {
            ConduitPortLayout conduitPortLayout = new ConduitPortLayout();
            conduitPortLayout.matrixPorts = new List<TileEntityPort>{
                new TileEntityPort(EntityPortType.All,Vector2Int.zero)
            };
            return conduitPortLayout;
        }

        public void onRightClick()
        {
            throw new System.NotImplementedException();
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            this.controller = matrixController;
        }
    }
}

