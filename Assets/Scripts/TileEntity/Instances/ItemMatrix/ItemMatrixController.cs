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

        public void addAutoCrafter(MatrixAutoCraftCore core) {

        }

        public void onInterfaceRemoved() {
            // There is probably a more efficent way to do this, not sure. 
            // Currently rebuilds entire system so its O(n) where n is the number of outputs in recipes
            // A very advanced system will have maybe 10,000 recipes? Assuming it takes about 0.00001ms per output, that takes 0.1 sec

            
        }

        public void removeFromSystem()
        {
            
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

