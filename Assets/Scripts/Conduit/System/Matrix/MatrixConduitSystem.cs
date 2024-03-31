using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;

namespace ConduitModule.Systems {
    public class MatrixConduitSystem : ConduitSystem<MatrixConduit>
    {
        private List<IMatrixConduitInteractable> tileEntities;
        public MatrixConduitSystem(string id) : base(id)
        {
            tileEntities = new List<IMatrixConduitInteractable>();
        }
        public override void addConduit(IConduit conduit)
        {
            base.addConduit(conduit);
            if (conduit is not MatrixConduit matrixConduit) {
                return;
            }
            addTileEntity(matrixConduit);

        }
        public override void rebuild()
        {
            tileEntities = new List<IMatrixConduitInteractable>();
            foreach (MatrixConduit matrixConduit in conduits) {
                addTileEntity(matrixConduit);
            }
            syncToController();
        }

        private void addTileEntity(MatrixConduit matrixConduit) {
            if (!matrixConduit.HasTileEntity) {
                return;
            } 
            if (matrixConduit.MatrixConduitInteractable is ItemMatrixController controller) {
                
            }
            tileEntities.Add(matrixConduit.MatrixConduitInteractable);
        }

        public void syncToController() {
            ItemMatrixController controller = null;
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                if (matrixConduitInteractable is ItemMatrixController controller1) {
                    if (controller != null) { // Hard Enforcement of only one controller per system
                        return;
                    }
                    controller = controller1;
                }
            }
            if (controller == null) {
                return;
            }
            controller.resetSystem();
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                matrixConduitInteractable.syncToController(controller);
            }
        }
    }
}

