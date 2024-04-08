using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Terminal")]
    public class MatrixTerminal : TileEntity, IMatrixConduitInteractable, IRightClickableTileEntity
    {
        private ItemMatrixController controller;
        [SerializeField] private ConduitPortLayout layout;

        public ItemMatrixController Controller { get => controller; }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void onRightClick()
        {
            if (controller == null) {
                return;
            }
            MatrixTerminalUI matrixTerminalUI = MatrixTerminalUI.newInstance();
            matrixTerminalUI.init(this);
            GlobalUIContainer.getInstance().getUiController().setGUI(matrixTerminalUI.gameObject);
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            this.controller = matrixController;
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            
        }
    }
}

