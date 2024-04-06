using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;
using ItemModule.Tags;
using ItemModule.Tags.Matrix;

namespace ConduitModule.Systems {
    public class MatrixConduitSystem : ConduitSystem<MatrixConduit>
    {
        private List<IMatrixConduitInteractable> tileEntities;
        
        private List<MatrixInterface> interfaces;
        private MatrixDriveCollection driveCollection;

        public List<MatrixInterface> Interfaces { get => interfaces;}
        public MatrixDriveCollection DriveCollection { get => driveCollection; }
        private List<MatrixAutoCraftCore> autoCraftingCores;
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
            tileEntities.Add(matrixConduit.MatrixConduitInteractable);
        }

        public void syncToController() {
            ItemMatrixController controller = null;
            controller = null;
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                if (matrixConduitInteractable is ItemMatrixController controller1) {
                    if (controller != null && !controller.Equals(controller1)) { // Hard Enforcement of only one controller per system
                        return;
                    }
                    controller = controller1;
                }
            }
            if (controller == null) {
                foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                    matrixConduitInteractable.syncToController(null);
                }
                return;
            }
            interfaces = new List<MatrixInterface>();
            driveCollection = new MatrixDriveCollection();
            autoCraftingCores = new List<MatrixAutoCraftCore>();
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                matrixConduitInteractable.syncToSystem(this);
                matrixConduitInteractable.syncToController(controller);
            }
        }

        public void addInterface(MatrixInterface matrixInterface) {
            interfaces.Add(matrixInterface);
        }
        public void setDrive(MatrixDrive matrixDrive) {
            driveCollection.setDrive(matrixDrive);
        }
        public void removeDrive(MatrixDrive matrixDrive) {
            driveCollection.removeDrive(matrixDrive);
        }

        public void addAutoCrafter(MatrixAutoCraftCore core) {
            autoCraftingCores.Add(core);
        }
        public void removeAutoCrafter(MatrixAutoCraftCore core) {
            if (autoCraftingCores.Contains(core)) {
                autoCraftingCores.Remove(core);
            }
        }
    }
}

