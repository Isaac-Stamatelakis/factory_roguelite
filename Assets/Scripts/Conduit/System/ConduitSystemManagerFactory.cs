using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using ConduitModule.Ports;

namespace ConduitModule.Systems {
    public static class ConduitSystemManagerFactory
    {
        public static IConduitSystemManager createManager(
            ConduitType conduitType, 
            IConduit[,] conduits,
            Vector2Int size,
            Dictionary<TileEntity, List<TileEntityPort>> chunkConduitPorts,
            Vector2Int referencePosition
        ) {
            bool isPort = conduitType == ConduitType.Item || conduitType == ConduitType.Fluid || conduitType == ConduitType.Energy || conduitType == ConduitType.Signal;
            if (isPort) {
                return new PortConduitSystemManager(
                    conduitType: conduitType,
                    conduits: castConduitArray<IPortConduit>(conduits,size),
                    size: size,
                    chunkConduitPorts: chunkConduitPorts,
                    referencePosition: referencePosition
                );
            }
            bool isMatrix = conduitType == ConduitType.Matrix;
            if (isMatrix) {
                return new MatrixConduitSystemManager(
                    conduitType: conduitType,
                    conduits: castConduitArray<MatrixConduit>(conduits,size),
                    size: size,
                    chunkConduitPorts: chunkConduitPorts,
                    referencePosition: referencePosition
                );
            }
            Debug.LogError("ConduitSystemManagerFactory did handle case for " + conduitType);
            return null;
        }

        private static T[,] castConduitArray<T>(IConduit[,] conduits, Vector2Int size) {
            T[,] tConduits = new T[size.x,size.y];
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    tConduits[x,y] = (T) conduits[x,y];
                }
            }
            return tConduits;
        }
    }
}

