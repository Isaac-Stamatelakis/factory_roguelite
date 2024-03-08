using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule;

namespace TileEntityModule.Instances.CompactMachines {
    public static class CompactMachineHelper 
    {
        /// 4*24^6 < |2^31-1| < |-2^31|
        private static int maxDepth = 6;
        public static int MaxDepth {get => maxDepth; }
        public static int[] xBounds;
        public static int[] yBounds;

        /// </summary>
        /// The map from position to depth+1 position is a bijection 
        /// <summary>
        public static Vector2Int getPositionInDim(Vector2Int position) {
            return getRingSize() * getDepth(position);
        }

        /// </summary>
        /// 
        /// <summary>
        public static Vector2Int getRingSize() {
            IntervalVector dim0Area = WorldCreation.getDim0Bounds();
            return seperationPerTile() * new Vector2Int(
                Mathf.Abs(dim0Area.X.LowerBound-dim0Area.X.UpperBound)+1,
                Mathf.Abs(dim0Area.Y.LowerBound-dim0Area.Y.UpperBound)+1
            ); 
        }

        public static int seperationPerTile() {
            return Global.ChunkSize;
        }

        public static int getDepth(Vector2Int position) {
            if (xBounds == null) {
                Interval<int> xSize = WorldCreation.getDim0Bounds().X;
                if (Mathf.Abs(xSize.LowerBound) != Mathf.Abs(xSize.UpperBound)) {
                    Debug.LogWarning("Compact Machine at position " + position + " bounds for x as bounds are not symetrical");
                }
                xBounds = initBounds(Mathf.Abs(xSize.LowerBound));
            }
            if (yBounds == null) {
                Interval<int> ySize = WorldCreation.getDim0Bounds().X;
                if (Mathf.Abs(ySize.LowerBound) != Mathf.Abs(ySize.UpperBound)) {
                    Debug.LogWarning("Compact Machine bounds for x as bounds are not symetrical");
                }
                yBounds = initBounds(Mathf.Abs(ySize.LowerBound));
            }
            for (int depth = 0; depth <= maxDepth; depth++) {
                bool inXBounds = Mathf.Abs(position.x) < xBounds[depth];
                bool inYBounds = Mathf.Abs(position.y) < yBounds[depth];
                if (!inXBounds && !inYBounds) {
                    continue;
                }
                if (inXBounds && inYBounds) {
                    return depth;
                } 
                if (inXBounds) {
                    Debug.LogWarning("Compact Machine at '" + position + "' is in ring " + depth + " x bounds but not y bounds");
                }
                if (inYBounds) {
                    Debug.LogWarning("Compact Machine at '" + position + "' is in ring " + depth + " y bounds but not x bounds");
                }
            }
            Debug.LogWarning("Compact Machine at '" + position + "' is outside depth bounds");
            return -1;
        }

        private static int[] initBounds(int size) {
            int[] bounds = new int[maxDepth+1];
            bounds[0] = size*seperationPerTile();
            for (int i = 1; i <= maxDepth; i++) {
                bounds[i] = bounds[i-1]*seperationPerTile();
            } 
            return bounds;
        }
        public static Vector2Int getParentPosition(CompactMachine compactMachine) {
            Vector2Int position = compactMachine.getCellPosition();
            return new Vector2Int(Mathf.FloorToInt(position.x/seperationPerTile()),Mathf.FloorToInt(position.y/seperationPerTile()));
        }
    }
}
