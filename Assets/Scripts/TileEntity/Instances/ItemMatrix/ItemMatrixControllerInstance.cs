using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Systems;
using UnityEngine;
using Items.Tags;
using Items.Tags.Matrix;
using Items;
using Conduits.Ports;


namespace TileEntityModule.Instances.Matrix {
    public class ItemMatrixControllerInstance : TileEntityInstance<ItemMatrixController>, IMatrixConduitInteractable
    {
        private HashSet<MatrixConduitSystem> systems = new HashSet<MatrixConduitSystem>();
        private MatrixRecipeCollection recipes = new MatrixRecipeCollection();
        public MatrixRecipeCollection Recipes { get => recipes;}
        public ItemMatrixControllerInstance(ItemMatrixController tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.Layout;
        }
        public void resetSystem() {
            
        }
        public void sendItem(ItemSlot toInsert) {
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                matrixConduitSystem.DriveCollection.send(toInsert);
                if (toInsert.itemObject == null || toInsert.amount == 0) {
                    return;
                }
            }
        }

        public ItemSlot takeItem(string id, ItemTagKey itemTagKey, int amount) {
            ItemSlot toReturn = null;
            int takeAmount = amount;
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                ItemSlot taken = matrixConduitSystem.DriveCollection.take(id,itemTagKey,takeAmount);
                if (taken == null || taken.itemObject == null) {
                    continue;
                }
                takeAmount -= taken.amount;
                if (toReturn == null) {
                    toReturn = taken;
                } else {
                    toReturn.amount += taken.amount; 
                }
                if (takeAmount == 0) {
                    return toReturn;
                } 
                if (takeAmount < 0) {
                    Debug.LogError("ItemMatrixController take item return took more than takeAmount");
                    return toReturn;
                }
            }
            return toReturn;
        }

        public int amountOf(string id, ItemTagKey itemTagKey) {
            int amount = 0;
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                amount += matrixConduitSystem.DriveCollection.amountOf(id,itemTagKey);
            }    
            return amount;
        }
        public MatrixDriveCollection getEntireDriveCollection() {
            MatrixDriveCollection matrixInventory = new MatrixDriveCollection();
            foreach (MatrixConduitSystem system in systems) {
                matrixInventory.merge(system.DriveCollection);
            }
            return matrixInventory;
        }
        public void syncToController(ItemMatrixControllerInstance matrixController)
        {
            if (!matrixController.Equals(this)) {
                return;
            }
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            systems.Add(matrixConduitSystem);
        }

        public void removeFromSystem()
        {
            
        }
        public HashSet<MatrixAutoCraftingCoreInstance> getAutoCraftControllers() {
            HashSet<MatrixAutoCraftingCoreInstance> cores = new HashSet<MatrixAutoCraftingCoreInstance>();
            foreach (MatrixConduitSystem system in systems) {
                foreach (MatrixAutoCraftingCoreInstance craftCore in system.AutoCraftingCores) {
                    cores.Add(craftCore);
                }
            }
            return cores;
        }
    }

    public class MatrixDriveInventory {
        public List<ItemSlot> inventories;
        public int maxSize;
        public MatrixDriveInventory(List<ItemSlot> inventories, int maxSize) {
            this.inventories = inventories;
            this.maxSize = maxSize;
        }
    }
}

