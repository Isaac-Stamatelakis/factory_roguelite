using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags;
using ItemModule.Tags.Matrix;
using ItemModule;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixItemCollection {
        private Stack<(ItemSlot,int)> notFull;
        private Stack<(ItemSlot,int)> full;

        public Stack<(ItemSlot, int)> NotFull { get => notFull; }
        public Stack<(ItemSlot, int)> Full { get => full; }
        public int TotalAmount {get => totalAmount;}
        private int totalAmount;
        public MatrixItemCollection() {
            notFull = new Stack<(ItemSlot, int)>();
            full = new Stack<(ItemSlot, int)>();
        }
        public void addMatrixDriveSlot(MatrixDriveInventory driveInventory, ItemSlot matrixDriveSlot) {
            if (matrixDriveSlot.amount > driveInventory.maxSize) {
                // If this somehow bugs, sorry you lose your items
                Debug.LogError("Tried to add matrix drive which had more items in it than its maxSize");
                matrixDriveSlot.amount = driveInventory.maxSize;
            }
            totalAmount += matrixDriveSlot.amount;
            if (matrixDriveSlot.amount == driveInventory.maxSize) {
                full.Push((matrixDriveSlot,driveInventory.maxSize));
            } else {
                notFull.Push((matrixDriveSlot,driveInventory.maxSize));
            }
        }
        public void sendItem(ItemSlot itemSlot) {
            if (notFull.Count == 0) {
                return;
            }
            (ItemSlot,int) value = notFull.Pop();
            ItemSlot driveSlot = value.Item1;
            int amountBefore = itemSlot.amount;
            ItemSlotHelper.insertIntoSlot(driveSlot,itemSlot,value.Item2);
            int difference = amountBefore-itemSlot.amount;
            totalAmount += difference;
            if (driveSlot.amount == value.Item2) {
                full.Push(value);
            } else {
                notFull.Push(value);
            }
        }

        public ItemSlot takeItem(int amount) {
            if (notFull.Count == 0 && full.Count == 0) {
                return null;
            }
            ItemSlot spliced = null;
            if (notFull.Count > 0) {
                spliced = ItemSlotFactory.splice(notFull.Peek().Item1,0);
            } else if (full.Count > 0) {
                spliced = ItemSlotFactory.splice(full.Peek().Item1,0);
            }
            while (spliced.amount < amount && notFull.Count > 0) {
                (ItemSlot,int) driveValue = notFull.Pop();
                ItemSlot driveItem = driveValue.Item1;
                ItemSlotHelper.insertIntoSlot(spliced,driveItem,amount);
                if (driveItem.itemObject != null && driveItem.amount > 0) {
                    notFull.Push(driveValue);
                    break;
                }
            }
            while (spliced.amount < amount && full.Count > 0) {
                (ItemSlot,int) driveValue = full.Pop();
                ItemSlot driveItem = driveValue.Item1;
                ItemSlotHelper.insertIntoSlot(spliced,driveItem,amount);
                if (driveItem.itemObject != null && driveItem.amount > 0) {
                    full.Push(driveValue);
                    break;
                }
            }
            totalAmount-= spliced.amount;
            return spliced;
        }
    }
    public class MatrixDriveCollection
    {
        private Dictionary<MatrixDrive, List<MatrixDriveInventory>> driveInventories;
        private Dictionary<string, Dictionary<ItemTagKey, MatrixItemCollection>> idTagItemDict;
        public MatrixDriveCollection() {
            driveInventories = new Dictionary<MatrixDrive, List<MatrixDriveInventory>>();
            idTagItemDict = new Dictionary<string, Dictionary<ItemTagKey, MatrixItemCollection>>();
        }
        
        public Dictionary<MatrixDrive, List<MatrixDriveInventory>> DriveInventories {get => driveInventories;}
        public void send(ItemSlot toInsert) {
            if (toInsert == null || toInsert.itemObject == null) {
                return;
            }
            ItemTagKey itemTagKey = new ItemTagKey(toInsert.tags);
            if (!idTagItemDict.ContainsKey(toInsert.itemObject.id) || !idTagItemDict[toInsert.itemObject.id].ContainsKey(itemTagKey)) {
                insertNewItem(toInsert,itemTagKey);
                return;
            }
            idTagItemDict[toInsert.itemObject.id][itemTagKey].sendItem(toInsert);
            if (toInsert.itemObject == null || toInsert.amount == 0) {
                return;
            }
            insertNewItem(toInsert,itemTagKey);
        }

        private void insertNewItem(ItemSlot itemSlot, ItemTagKey itemTagKey) {
            foreach (List<MatrixDriveInventory> matrixDriveInventoryList in driveInventories.Values) {
                foreach (MatrixDriveInventory matrixDriveInventory in matrixDriveInventoryList) {
                    for (int i = 0; i < matrixDriveInventory.inventories.Count; i++) {
                        ItemSlot driveSlot = matrixDriveInventory.inventories[i];
                        if (driveSlot != null && driveSlot.itemObject != null) {
                            continue;
                        }
                        if (!idTagItemDict.ContainsKey(itemSlot.itemObject.id)) {
                            idTagItemDict[itemSlot.itemObject.id] = new Dictionary<ItemTagKey, MatrixItemCollection>();
                        }
                        if (!idTagItemDict[itemSlot.itemObject.id].ContainsKey(itemTagKey)) {
                            idTagItemDict[itemSlot.itemObject.id][itemTagKey] = new MatrixItemCollection();
                        }
                        ItemSlot newSlot = ItemSlotFactory.copy(itemSlot);
                        matrixDriveInventory.inventories[i] = newSlot;
                        itemSlot.itemObject = null;
                        itemSlot.amount = 0;
                        idTagItemDict[newSlot.itemObject.id][itemTagKey].addMatrixDriveSlot(matrixDriveInventory,newSlot);
                        return;
                    }
                    
                }
            }
        }
        public ItemSlot take(string id, ItemTagKey itemTagKey, int amount) {
            if (!idTagItemDict.ContainsKey(id) || !idTagItemDict[id].ContainsKey(itemTagKey)) {
                return null;
            }
            return idTagItemDict[id][itemTagKey].takeItem(amount);
        }

        public int amountOf(string id, ItemTagKey itemTagKey) {
            if (!idTagItemDict.ContainsKey(id) || !idTagItemDict[id].ContainsKey(itemTagKey)) {
                return 0;
            }
            return idTagItemDict[id][itemTagKey].TotalAmount;
        }
        
        public void merge(MatrixDriveCollection matrixDriveCollection) {
            foreach (KeyValuePair<MatrixDrive, List<MatrixDriveInventory>> kvp in matrixDriveCollection.DriveInventories) {
                driveInventories[kvp.Key] = kvp.Value;
            }
            rebuildDict();
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
                MatrixDriveInventory matrixDriveInventory = new MatrixDriveInventory(
                    inventories: (List<ItemSlot>)drive.tags.Dict[ItemTag.StorageDrive],
                    maxSize: matrixDriveItem.MaxAmount
                );
                addMatrixDriveInventoryToDict(matrixDriveInventory);
                inventories.Add(matrixDriveInventory);
            }
            driveInventories[matrixDrive] = inventories;
        }

        public void removeDrive(MatrixDrive matrixDrive) {
            if (driveInventories.ContainsKey(matrixDrive)) {
                driveInventories.Remove(matrixDrive);
                rebuildDict();
            }
        }

        public void rebuildDict() {
            idTagItemDict = new Dictionary<string, Dictionary<ItemTagKey, MatrixItemCollection>>();
            foreach (List<MatrixDriveInventory> matrixDriveInventoryList in driveInventories.Values) {
                foreach (MatrixDriveInventory matrixDriveInventory in matrixDriveInventoryList) {
                    addMatrixDriveInventoryToDict(matrixDriveInventory);
                }
            }
        }

        private void addMatrixDriveInventoryToDict(MatrixDriveInventory matrixDriveInventory) {
            for (int i = 0; i < matrixDriveInventory.inventories.Count; i++) {
                ItemSlot driveSlot = matrixDriveInventory.inventories[i];
                if (driveSlot == null || driveSlot.itemObject == null) {
                    continue;
                }
                ItemTagKey itemTagKey = new ItemTagKey(driveSlot.tags);
                insertItemIntoDict(matrixDriveInventory,driveSlot,itemTagKey);
            }
        }

        private void insertItemIntoDict(MatrixDriveInventory matrixDriveInventory, ItemSlot itemSlot, ItemTagKey itemTagKey) {
            if (!idTagItemDict.ContainsKey(itemSlot.itemObject.id)) {
                idTagItemDict[itemSlot.itemObject.id] = new Dictionary<ItemTagKey, MatrixItemCollection>();
            }
            if (!idTagItemDict[itemSlot.itemObject.id].ContainsKey(itemTagKey)) {
                idTagItemDict[itemSlot.itemObject.id][itemTagKey] = new MatrixItemCollection();
            }
            idTagItemDict[itemSlot.itemObject.id][itemTagKey].addMatrixDriveSlot(matrixDriveInventory,itemSlot);
        }


    }
}

