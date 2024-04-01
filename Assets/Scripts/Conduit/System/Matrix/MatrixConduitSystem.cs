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
        private List<MatrixDriveInventory> driveInventories;

        public List<MatrixInterface> Interfaces { get => interfaces;}
        public List<MatrixDriveInventory> DriveInventories { get => driveInventories; }

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
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                if (matrixConduitInteractable is ItemMatrixController controller1) {
                    if (controller != null && !controller.Equals(controller1)) { // Hard Enforcement of only one controller per system
                        return;
                    }
                    controller = controller1;
                }
            }
            if (controller == null) {
                return;
            }
            interfaces = new List<MatrixInterface>();
            driveInventories = new List<MatrixDriveInventory>();
            foreach (IMatrixConduitInteractable matrixConduitInteractable in tileEntities) {
                matrixConduitInteractable.syncToSystem(this);
                matrixConduitInteractable.syncToController(controller);
            }
        }

        public void addInterface(MatrixInterface matrixInterface) {
            interfaces.Add(matrixInterface);
        }
        public void addDrive(MatrixDrive matrixDrive) {
            foreach (ItemSlot drive in matrixDrive.StorageDrives) {
                if (
                    drive == null || 
                    drive.itemObject == null || 
                    drive.tags == null || 
                    !drive.tags.Dict.ContainsKey(ItemTag.StorageDrive) || 
                    drive.itemObject is not MatrixDriveItem matrixDriveItem
                ) {
                    continue;
                }
                driveInventories.Add(new MatrixDriveInventory(
                    (List<ItemSlot>)drive.tags.Dict[ItemTag.StorageDrive],
                    matrixDriveItem.MaxAmount
                ));
            }
        }
    }
}

