using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags;
using ItemModule.Tags.Matrix;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixDriveCollection
    {
        private Dictionary<MatrixDrive, List<MatrixDriveInventory>> driveInventories;
        public MatrixDriveCollection() {
            driveInventories = new Dictionary<MatrixDrive, List<MatrixDriveInventory>>();
        }
        public Dictionary<MatrixDrive, List<MatrixDriveInventory>> DriveInventories {get => driveInventories;}
        public void send(ItemSlot toInsert) {
            foreach (List<MatrixDriveInventory> matrixDriveInventoryList in driveInventories.Values) {  
                foreach (MatrixDriveInventory matrixDriveInventory in matrixDriveInventoryList) {
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
        }
        public void take() {

        }
        
        public void merge(MatrixDriveCollection matrixDriveCollection) {
            foreach (KeyValuePair<MatrixDrive, List<MatrixDriveInventory>> kvp in matrixDriveCollection.DriveInventories) {
                driveInventories[kvp.Key] = kvp.Value;
            }
        }
        
        public Queue<(MatrixDrive,Queue<MatrixDriveInventory>)> getQueueOfDrives() {
            Queue<(MatrixDrive,Queue<MatrixDriveInventory>)> matrixDrives = new Queue<(MatrixDrive,Queue<MatrixDriveInventory>)>();
            foreach (KeyValuePair<MatrixDrive,List<MatrixDriveInventory>> kvp in driveInventories) {
                Queue<MatrixDriveInventory> queue = new Queue<MatrixDriveInventory>();
                foreach (MatrixDriveInventory driveInventory in kvp.Value) {
                    queue.Enqueue(driveInventory);
                }
                matrixDrives.Enqueue((kvp.Key,queue));
            }
            return matrixDrives;
        }
        public void setDrive(MatrixDrive matrixDrive) {
            List<MatrixDriveInventory> inventories = new List<MatrixDriveInventory>();
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
                inventories.Add(new MatrixDriveInventory(
                    (List<ItemSlot>)drive.tags.Dict[ItemTag.StorageDrive],
                    matrixDriveItem.MaxAmount
                ));
            }
            driveInventories[matrixDrive] = inventories;
        }

        public void removeDrive(MatrixDrive matrixDrive) {
            if (driveInventories.ContainsKey(matrixDrive)) {
                driveInventories.Remove(matrixDrive);
            }
        }


    }
}

