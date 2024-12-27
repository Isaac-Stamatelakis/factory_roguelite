using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Systems;
using UnityEngine;
using Items.Tags;
using Items.Tags.Matrix;
using Items;
using Conduits.Ports;


namespace TileEntity.Instances.Matrix {
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
            return TileEntityObject.Layout;
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

        public ItemSlot TakeItem(string id, ItemTagKey itemTagKey, uint amount) {
            ItemSlot toReturn = null;
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                ItemSlot taken = matrixConduitSystem.DriveCollection.Take(id,itemTagKey,amount);
                if (taken == null || taken.itemObject == null) {
                    continue;
                }
                amount -= taken.amount;
                if (toReturn == null) {
                    toReturn = taken;
                } else {
                    toReturn.amount += taken.amount; 
                }
                if (amount == 0) {
                    return toReturn;
                }
            }
            return toReturn;
        }

        public uint AmountOf(string id, ItemTagKey itemTagKey) {
            uint amount = 0;
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                amount += matrixConduitSystem.DriveCollection.AmountOf(id,itemTagKey);
            }    
            return amount;
        }
        public MatrixDriveCollection GetEntireDriveCollection() {
            MatrixDriveCollection matrixInventory = new MatrixDriveCollection();
            foreach (MatrixConduitSystem system in systems) {
                matrixInventory.merge(system.DriveCollection);
            }
            return matrixInventory;
        }
        public void SyncToController(ItemMatrixControllerInstance matrixController)
        {
            if (!matrixController.Equals(this)) {
                return;
            }
        }

        public void SyncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            systems.Add(matrixConduitSystem);
        }

        public void RemoveFromSystem()
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
        public uint maxSize;
        public MatrixDriveInventory(List<ItemSlot> inventories, uint maxSize) {
            this.inventories = inventories;
            this.maxSize = maxSize;
        }
    }
}

