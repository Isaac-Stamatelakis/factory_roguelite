using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine.Tilemaps;
using Items.Tags;
using Items.Tags.Matrix;
using Items;
using UI;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixTerminalInstance : TileEntityInstance<MatrixTerminal>, IMatrixConduitInteractable, IRightClickableTileEntity
    {
        public ItemMatrixControllerInstance Controller { get => controller; }
        private ItemMatrixControllerInstance controller;
        public MatrixTerminalInstance(MatrixTerminal tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.Layout;
        }

        public void onRightClick()
        {
            if (controller == null) {
                return;
            }
            MatrixTerminalUI matrixTerminalUI = MatrixTerminalUI.newInstance();
            matrixTerminalUI.init(this);
            MainCanvasController.Instance.DisplayObject(matrixTerminalUI.gameObject);
        }

        public void removeFromSystem()
        {
            
        }

        public void syncToController(ItemMatrixControllerInstance matrixController)
        {
            this.controller = matrixController;
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            
        }
    }
}

