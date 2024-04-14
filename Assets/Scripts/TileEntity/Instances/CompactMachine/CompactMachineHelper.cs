using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;
using DimensionModule;
using WorldModule.Generation;
using ConduitModule.Ports;

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
            CompactMachineDimController dimController = DimensionManagerContainer.getManager().GetCompactMachineDimController();
            return dimController.hasSystemOfCompactMachine(compactMachine);
        }

        /// </summary>
        /// Maps a port inside a compact machine to its port on the compact machine tile entity
        /// <summary>
        public static Vector2Int getPortPositionInLayout(Vector2Int relativePortPosition, ConduitPortLayout layout, ConduitType type) {
            List<TileEntityPort> possiblePorts = null;
            switch (type) {
                case ConduitType.Item:
                    possiblePorts = layout.itemPorts;
                    break;
                case ConduitType.Energy:
                    possiblePorts = layout.energyPorts;
                    break;
                case ConduitType.Fluid:
                    possiblePorts = layout.fluidPorts;
                    break;
                case ConduitType.Signal:
                    possiblePorts = layout.signalPorts;
                    break;
            }
            float smallestDistance = float.PositiveInfinity;
            TileEntityPort closestPort = null;
            foreach (TileEntityPort port in possiblePorts) {
                // maps port position to the center of its relative chunk (eg (1,1) -> (36,36))
                Vector2 positionInSideCompactMachine =  (port.position + Vector2.one/2f) * (Global.ChunkSize); 
                float dist = Vector2.Distance(positionInSideCompactMachine,relativePortPosition);
                if (dist < smallestDistance) {
                    smallestDistance = dist;
                    closestPort = port;
                }
            }
            if (closestPort == null) {
                Debug.LogError("Could not find port to map compact machine to");
                return Vector2Int.zero;
            }
            return closestPort.position;
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
            DimensionManager dimensionManager = DimensionManagerContainer.getManager();
            if (depth == 0) {
                dimensionManager.setActiveSystemFromCellPosition(0,compactMachine.getCellPosition());
                dimensionManager.setPlayerPositionFromCell(compactMachine.getCellPosition());
            } else {
                Vector2Int parentPosition = compactMachine.getCellPosition()/24;
                dimensionManager.setActiveSystemFromCellPosition(1,parentPosition);
                dimensionManager.setPlayerPositionFromCell(compactMachine.getCellPosition());
            }
            
            
        }
        public static void teleportIntoCompactMachine(CompactMachine compactMachine) {
            DimensionManager dimensionManager = DimensionManagerContainer.getManager();
            dimensionManager.setActiveSystemFromCellPosition(1,compactMachine.getCellPosition());
            dimensionManager.setPlayerPositionFromCell(compactMachine.getTeleporterPosition());
        }
    }
}
