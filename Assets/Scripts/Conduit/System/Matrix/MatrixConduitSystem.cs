using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Matrix;
using Items.Tags;
using Items.Tags.Matrix;
using TileEntity;

namespace Conduits.Systems {
    public class MatrixConduitSystem : ConduitSystem<MatrixConduit>
    {
        private List<IMatrixConduitInteractable> tileEntities;
        
        private HashSet<MatrixInterfaceInstance> interfaces;
        private MatrixDriveCollection driveCollection;
        public HashSet<MatrixInterfaceInstance> Interfaces { get => interfaces;}
        public MatrixDriveCollection DriveCollection { get => driveCollection; }
        public HashSet<MatrixAutoCraftingCoreInstance> AutoCraftingCores { get => autoCraftingCores; }

        private HashSet<MatrixAutoCraftingCoreInstance> autoCraftingCores;
        private ItemMatrixControllerInstance controller;
        public MatrixConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
        {
            tileEntities = new List<IMatrixConduitInteractable>();
        }
        public override void AddConduit(IConduit conduit)
        {
            base.AddConduit(conduit);
            if (conduit is not MatrixConduit matrixConduit) {
                return;
            }
            addTileEntity(matrixConduit);

        }
        public override void Rebuild()
        {
            tileEntities = new List<IMatrixConduitInteractable>();
            foreach (MatrixConduit matrixConduit in conduits) {
                addTileEntity(matrixConduit);
            }
            syncToController();
        }

        public override bool IsActive()
        {
            return controller != null;
        }

        /// <summary>
        /// Rebuilds sections of the system depending on the TileEntity
        /// </summary>
        public void addTileEntityToSystem(MatrixConduit matrixConduit, IMatrixConduitInteractable matrixConduitInteractable) {
            matrixConduitInteractable.SyncToSystem(this);
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
                if (matrixConduitInteractable is ItemMatrixControllerInstance controller1) {
                    if (controller != null && !controller.Equals(controller1)) { // Hard Enforcement of only one controller per system
                        return;
                    }
                    controller = controller1;
                }
            }
            if (controller == null) {
                foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                    matrixConduitInteractable.SyncToController(null);
                }
                return;
            }
            interfaces = new HashSet<MatrixInterfaceInstance>();
            driveCollection = new MatrixDriveCollection();
            autoCraftingCores = new HashSet<MatrixAutoCraftingCoreInstance>();
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                matrixConduitInteractable.SyncToSystem(this);
                matrixConduitInteractable.SyncToController(controller);
            }
        }

        public void addInterface(MatrixInterfaceInstance matrixInterface) {
            interfaces.Add(matrixInterface);
            if (controller != null) {
                controller.Recipes.addInterface(matrixInterface);
            }

        }

        public void removeInterface(MatrixInterfaceInstance matrixInterface) {
            interfaces.Remove(matrixInterface);
            if (controller != null) {
                controller.Recipes.removeInterface(matrixInterface);
            }
            
        }
        public void setDrive(MatrixDriveInstance matrixDrive) {
            driveCollection.setDrive(matrixDrive);
        }
        public void removeDrive(MatrixDriveInstance matrixDrive) {
            driveCollection.removeDrive(matrixDrive);
        }

        public void addAutoCrafter(MatrixAutoCraftingCoreInstance core) {
            autoCraftingCores.Add(core);
        }
        public void removeAutoCrafter(MatrixAutoCraftingCoreInstance core) {
            if (autoCraftingCores.Contains(core)) {
                autoCraftingCores.Remove(core);
            }
        }
    }
}

