using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixAutoCraftCore : TileEntity, IMatrixConduitInteractable, IMultiBlockTileEntity
    {
        [SerializeField] private ConduitPortLayout layout;
        private ItemMatrixController controller;
        private int totalMemory;
        private int totalProcessors;

        public int TotalMemory { get => totalMemory; set => totalMemory = value; }
        public int TotalProcessors { get => totalProcessors; set => totalProcessors = value; }

        public void assembleMultiBlock()
        {
            HashSet<IMatrixCraftTile> connectedCraftTiles = new HashSet<IMatrixCraftTile>();
            TileEntityHelper.dfsTileEntity(this,connectedCraftTiles); 
            // Verify shape is rectangular
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
            // Add one to account for core, if tileCount + 1 == size.x*size.y, then the shape is rectangular
            bool isRectangular = connectedCraftTiles.Count+1 == size.x*size.y; 
            if (!isRectangular) {
                return;
            }
            foreach (IMatrixCraftTile matrixCraftTile in connectedCraftTiles) {
                matrixCraftTile.sync(this);
            }  
        }

        public ConduitPortLayout getConduitPortLayout()
        {
            return layout;
        }

        public void syncToController(ItemMatrixController matrixController)
        {
            
        }

        public void syncToSystem(MatrixConduitSystem matrixConduitSystem)
        {
            // An autocrafter with no memory and no processors cannot do anything
            bool canFunction = totalMemory > 0 && totalProcessors > 0;
            if (!canFunction) {
                return;
            }
            matrixConduitSystem.addAutoCrafter(this);
        }
    }
}

