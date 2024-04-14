using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using UnityEngine;
using ConduitModule.Systems;
using UnityEngine.Tilemaps;
using ChunkModule;
using ItemModule.Tags;
using ItemModule.Tags.Matrix;
using ItemModule;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Controller", menuName = "Tile Entity/Item Matrix/Controller")]
    public class ItemMatrixController : TileEntity, IMatrixConduitInteractable
    {
        [SerializeField] private ConduitPortLayout layout;
        private HashSet<MatrixConduitSystem> systems;
        private MatrixRecipeCollection recipes;

        public MatrixRecipeCollection Recipes { get => recipes;}

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }
        public void resetSystem() {
            
        }


        public override void initalize(Vector2Int tilePosition, TileBase tileBase, IChunk chunk)
        {
            base.initalize(tilePosition, tileBase, chunk);
            systems = new HashSet<MatrixConduitSystem>();
            recipes = new MatrixRecipeCollection();
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
        public void syncToController(ItemMatrixController matrixController)
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
        public HashSet<MatrixAutoCraftCore> getAutoCraftControllers() {
            HashSet<MatrixAutoCraftCore> cores = new HashSet<MatrixAutoCraftCore>();
            foreach (MatrixConduitSystem system in systems) {
                foreach (MatrixAutoCraftCore craftCore in system.AutoCraftingCores) {
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

