using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using Conduits.Ports;
using Conduits.Systems;

namespace TileEntity.Instances.Matrix {
    public class MatrixAutoCraftingCoreInstance : MatrixAutoCraftingChassisInstance<MatrixAutoCraftCore>, IMatrixConduitInteractable, IMultiBlockTileEntity
    {
        private int totalMemory;
        private int totalProcessors;
        private bool assembled = false;
        public bool Assembled {get => assembled;}

        public int TotalMemory { get => totalMemory; set => totalMemory = value; }
        public int TotalProcessors { get => totalProcessors; set => totalProcessors = value; }

        private MatrixConduitSystem matrixConduitSystem;

        public MatrixAutoCraftingCoreInstance(MatrixAutoCraftCore tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

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
            foreach (ITileEntityInstance tileEntity in connectedCraftTiles) {
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
            return TileEntityObject.Layout;
        }

        public override void sync(MatrixAutoCraftingCoreInstance core)
        {
            if (!core.Equals(this)) {
                assembled = false;
            }
            this.core = core;
        }

        public void SyncToController(ItemMatrixControllerInstance matrixController)
        {
            
        }

        public void SyncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            this.matrixConduitSystem = matrixConduitSystem;
            if (assembled) {
                matrixConduitSystem.addAutoCrafter(this);
            }
        }

        public void RemoveFromSystem()
        {
            if (matrixConduitSystem != null) {
                matrixConduitSystem.removeAutoCrafter(this);
            }
        }
    }

}
