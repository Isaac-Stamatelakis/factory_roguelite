using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using ConduitModule.Systems;
using UnityEngine.Tilemaps;
using ChunkModule;
using ItemModule.Tags;
using ItemModule.Tags.Matrix;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Controller")]
    public class ItemMatrixController : TileEntity, IMatrixConduitInteractable
    {
        [SerializeField] private ConduitPortLayout layout;
        private List<MatrixInterface> interfaces;
        private List<MatrixDriveInventory> driveInventories;
        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
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

        public void resetSystem() {
            interfaces = new List<MatrixInterface>();
            driveInventories = new List<MatrixDriveInventory>();
        }

        public void addInterface(MatrixInterface matrixInterface) {
            interfaces.Add(matrixInterface);
        }

    

        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
        }

    
        public void sendItem(ItemSlot toInsert) {
            foreach (MatrixDriveInventory matrixDriveInventory in driveInventories) {  
                foreach (ItemSlot itemSlot in matrixDriveInventory.inventories) {
                    if (!ItemSlotHelper.canInsertIntoSlot(itemSlot,toInsert,matrixDriveInventory.maxSize)) {
                        continue;
                    }
                    ItemSlotHelper.insertIntoSlot(itemSlot,toInsert,matrixDriveInventory.maxSize);
                    if (itemSlot.amount <= 0) {
                        itemSlot.itemObject = null;
                        return;
                    }
                }
            }
        }
        public void syncToController(ItemMatrixController matrixController)
        {
            if (!matrixController.Equals(this)) {
                return;
            }
            Debug.Log("Controller synced to system");
        }

        private class MatrixDriveInventory {
            public List<ItemSlot> inventories;
            public int maxSize;
            public MatrixDriveInventory(List<ItemSlot> inventories, int maxSize) {
                this.inventories = inventories;
                this.maxSize = maxSize;
            }
        }
    }
}

