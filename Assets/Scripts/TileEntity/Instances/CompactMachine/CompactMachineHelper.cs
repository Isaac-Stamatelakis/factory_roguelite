using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using DimensionModule;
using WorldModule.Generation;

namespace TileEntityModule.Instances.CompactMachines {
    public static class CompactMachineHelper 
    {
        /// 4*24^6 < |2^31-1| < |-2^31|
        private static int maxDepth = 4;
        public static int MaxDepth {get => maxDepth; }

        /// </summary>
        /// The map from position to depth+1 position is a bijection 
        /// <summary>
        public static Vector2Int getPositionInDim(Vector2Int position) {
            return getRingBoundaryFromPosition(position)+position*seperationPerTile();
        }

        public static Vector2Int getPositionInNextRing(Vector2Int position) {
            return position*seperationPerTile();
        }

        /// </summary>
        /// 
        /// <summary>
        public static Vector2Int getRingSizeInChunks() {
            IntervalVector dim0Area = WorldCreation.getDim0Bounds();
            return new Vector2Int(
                Mathf.Abs(dim0Area.X.LowerBound-dim0Area.X.UpperBound)+1,
                Mathf.Abs(dim0Area.Y.LowerBound-dim0Area.Y.UpperBound)+1
            ); 
        }

        public static Vector2Int getRingBoundaryFromPosition(Vector2Int position) {
            int depth = getDepth(position);
            int val = 1;
            for (int i = 0; i < depth; i++) { // ring boundary increases expontentially with depth
                val *= seperationPerTile();
            }
            int signX = position.x >= 0 ? 1 : -1;
            int signY = position.y >= 0 ? 1 : -1;

            Vector2Int boundary = getBaseBoundary(signX,signY);
            Vector2Int dir = new Vector2Int(signX,signY);
            return dir * boundary * val;
        }

        public static int getDepth(Vector2Int position) {

            int signX = position.x >= 0 ? 1 : -1;
            int signY = position.y >= 0 ? 1 : -1;
            Vector2Int boundary = getBaseBoundary(signX,signY);
            int xBoundary = boundary.x;
            int xDepth = 0;
            for (int depth = 0; depth < Global.MaxSize; depth++) {
                if (position.x < xBoundary) {
                    xDepth = depth;
                    break;
                }
                xBoundary *= seperationPerTile();
            }
            int yBoundary = boundary.y;
            int yDepth = 0;
            for (int depth = 0; depth < Global.MaxSize; depth++) {
                if (position.x < xBoundary) {
                    yDepth = depth;
                    break;
                }
                xBoundary *= seperationPerTile();
            }
            return Mathf.Max(xDepth,yDepth);
        }

        public static Vector2Int getBaseBoundary(int signX, int signY) {
            Vector2Int size = getRingSizeInChunks();
            int x = signX == 1 ? (size.x + 1) / 2 : size.x/2;
            int y = signY == 1 ? (size.y + 1) / 2 : size.y/2;
            Vector2Int dir = new Vector2Int(signX, signY);
            Vector2Int boundary = new Vector2Int(x,y) * seperationPerTile();
            return boundary;
        }
        public static int seperationPerTile() {
            return Global.ChunkSize;
        }

        public static Vector2Int getParentPosition(CompactMachine compactMachine) {
            Vector2Int position = compactMachine.getCellPosition();
            return new Vector2Int(Mathf.FloorToInt(position.x/seperationPerTile()),Mathf.FloorToInt(position.y/seperationPerTile()));
        }

        public static bool isCreated(CompactMachine compactMachine) {
            CompactMachineDimController dimController = DimensionManagerContainer.getInstance().getManager().GetCompactMachineDimController();
            return dimController.hasSystemOfCompactMachine(compactMachine);
        }

        public static void initalizeCompactMachineSystem(CompactMachine compactMachine) {
            IntervalVector bounds = getCompactMachineBounds(compactMachine);
            WorldTileConduitData systemData = WorldCreation.prefabToWorldTileConduitData(compactMachine.tilemapContainer,bounds);
            WorldGenerationFactory.saveToJson(systemData,bounds.getSize(),bounds,1);
            Debug.Log(compactMachine.name + " Closed Chunk System Generated");
        }

        public static IntervalVector getCompactMachineBounds(CompactMachine compactMachine) {
            IntervalVector bounds = WorldCreation.getTileMapChunkBounds(compactMachine.tilemapContainer);
            bounds.add(compactMachine.getCellPosition());
            return bounds;
        }

        public static void teleportOutOfCompactMachine(CompactMachine compactMachine) {
            int depth = getDepth(compactMachine.getCellPosition());
            DimensionManager dimensionManager = DimensionManagerContainer.getInstance().getManager();
            if (depth == 0) {
                dimensionManager.setActiveSystemFromCellPosition(0,compactMachine.getCellPosition());
            } else {
                dimensionManager.setActiveSystemFromCellPosition(1,compactMachine.getCellPosition());
            }
            dimensionManager.setPlayerPositionFromCell(compactMachine.getCellPosition());
            
        }
        public static void teleportIntoCompactMachine(CompactMachine compactMachine) {
            DimensionManager dimensionManager = DimensionManagerContainer.getInstance().getManager();
            dimensionManager.setActiveSystemFromCellPosition(1,compactMachine.getCellPosition());
            dimensionManager.setPlayerPositionFromCell(compactMachine.getTeleporterPosition());
        }
    }
}
