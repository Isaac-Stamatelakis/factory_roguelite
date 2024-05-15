using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using Conduits.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Core", menuName = "Tile Entity/Item Matrix/Crafting/Core")]
    public class MatrixAutoCraftCore : MatrixAutoCraftingChassis, IMatrixConduitInteractable, IMultiBlockTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        private int totalMemory;
        private int totalProcessors;
        private bool assembled = false;
        public bool Assembled {get => assembled;}

        public int TotalMemory { get => totalMemory; set => totalMemory = value; }
        public int TotalProcessors { get => totalProcessors; set => totalProcessors = value; }

        private MatrixConduitSystem matrixConduitSystem;

        public void assembleMultiBlock()
        {
            if (matrixConduitSystem != null) {
                matrixConduitSystem.removeAutoCrafter(this);
            }
            totalMemory = 0;
            totalProcessors = 0;
            HashSet<IMatrixCraftTile> connectedCraftTiles = new HashSet<IMatrixCraftTile>();
            TileEntityHelper.dfsTileEntity(this,connectedCraftTiles); 
            foreach (IMatrixCraftTile craftTile in connectedCraftTiles) {
                craftTile.deactivate();
            }
            Dim2Bounds bounds = new Dim2Bounds(0,0,0,0);
            Vector2Int corePosition = getCellPosition();
            foreach (TileEntity tileEntity in connectedCraftTiles) {
                Vector2Int cellPosition = tileEntity.getCellPosition();
                Vector2Int dif = corePosition - cellPosition;
                if (dif.x < bounds.XLowerBound) {
                    bounds.XLowerBound = dif.x;
                } else if (dif.x > bounds.XUpperBound) {
                    bounds.XUpperBound = dif.x;
                }
                if (dif.y < bounds.YLowerBound) {
                    bounds.YLowerBound = dif.y;
                } else if (dif.y > bounds.YUpperBound) {
                    bounds.YUpperBound = dif.y;
                }
            }
            Vector2Int size = bounds.size();
            bool isRectangular = connectedCraftTiles.Count == size.x*size.y; 
            if (!isRectangular) {
                return;
            }
            assembled = true;
            foreach (IMatrixCraftTile matrixCraftTile in connectedCraftTiles) {
                matrixCraftTile.sync(this);
            }  
            // Will only not be assembled after this point if another core is within dfs search
            bool functionable = totalMemory > 0 && totalProcessors > 0;
            assembled = assembled && functionable;
            if (!assembled) {
                return;
            }
            foreach (IMatrixCraftTile matrixCraftTile in connectedCraftTiles) {
                matrixCraftTile.load();
            }
            if (matrixConduitSystem != null) {
                matrixConduitSystem.addAutoCrafter(this);
            }
            
        }
        
       

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public override void sync(MatrixAutoCraftCore core)
        {
            if (!core.Equals(this)) {
                assembled = false;
            }
            this.core = core;
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            this.matrixConduitSystem = matrixConduitSystem;
            if (assembled) {
                matrixConduitSystem.addAutoCrafter(this);
            }
        }

        public void removeFromSystem()
        {
            if (matrixConduitSystem != null) {
                matrixConduitSystem.removeAutoCrafter(this);
            }
        }
    }
}

