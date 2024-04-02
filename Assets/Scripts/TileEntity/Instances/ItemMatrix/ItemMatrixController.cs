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
        private HashSet<MatrixConduitSystem> systems;
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
        }

    
        public void sendItem(ItemSlot toInsert) {
            foreach (MatrixConduitSystem matrixConduitSystem in systems) {
                foreach (List<MatrixDriveInventory> matrixDriveInventoryList in matrixConduitSystem.DriveInventories.Values) {  
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
        }

        public List<ItemSlot> getItemSlots() {
            List<ItemSlot> inventories = new List<ItemSlot>();
            foreach (MatrixConduitSystem system in systems) {
                foreach (List<MatrixDriveInventory> matrixDriveInventoryList in system.DriveInventories.Values) {
                    foreach (MatrixDriveInventory driveInventory in matrixDriveInventoryList) {
                        inventories.AddRange(driveInventory.inventories);
                    }
                }
            }
            return inventories;
        }

        public Dictionary<MatrixDrive, List<MatrixDriveInventory>> getInventory() {
            Dictionary<MatrixDrive, List<MatrixDriveInventory>> totalDriveInventories = new Dictionary<MatrixDrive, List<MatrixDriveInventory>>();
            foreach (MatrixConduitSystem system in systems) {
                foreach (KeyValuePair<MatrixDrive, List<MatrixDriveInventory>> kvp in system.DriveInventories) {
                    totalDriveInventories[kvp.Key] = kvp.Value;
                }
            }
            return totalDriveInventories;
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

        public void addAutoCrafter(MatrixAutoCraftCore core) {

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

