using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Matrix;
using Items.Tags;
using Items.Tags.Matrix;
using TileEntityModule;

namespace Conduits.Systems {
    public class MatrixConduitSystem : ConduitSystem<MatrixConduit>
    {
        private List<IMatrixConduitInteractable> tileEntities;
        
        private HashSet<MatrixInterface> interfaces;
        private MatrixDriveCollection driveCollection;

        public HashSet<MatrixInterface> Interfaces { get => interfaces;}
        public MatrixDriveCollection DriveCollection { get => driveCollection; }
        public HashSet<MatrixAutoCraftCore> AutoCraftingCores { get => autoCraftingCores; }

        private HashSet<MatrixAutoCraftCore> autoCraftingCores;
        private ItemMatrixController controller;
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

        /// <summary>
        /// Rebuilds sections of the system depending on the TileEntity
        /// </summary>
        public void addTileEntityToSystem(MatrixConduit matrixConduit, IMatrixConduitInteractable matrixConduitInteractable) {
            matrixConduitInteractable.syncToSystem(this);
        }
        private void addTileEntity(MatrixConduit matrixConduit) {
            if (!matrixConduit.HasTileEntity) {
                return;
            } 
            tileEntities.Add(matrixConduit.MatrixConduitInteractable);
        }

        public void syncToController() {
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
            interfaces = new HashSet<MatrixInterface>();
            driveCollection = new MatrixDriveCollection();
            autoCraftingCores = new HashSet<MatrixAutoCraftCore>();
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                matrixConduitInteractable.syncToSystem(this);
                matrixConduitInteractable.syncToController(controller);
            }
        }

        public void addInterface(MatrixInterface matrixInterface) {
            interfaces.Add(matrixInterface);
            if (controller != null) {
                controller.Recipes.addInterface(matrixInterface);
            }

        }

        public void removeInterface(MatrixInterface matrixInterface) {
            interfaces.Remove(matrixInterface);
            if (controller != null) {
                controller.Recipes.removeInterface(matrixInterface);
            }
            
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

