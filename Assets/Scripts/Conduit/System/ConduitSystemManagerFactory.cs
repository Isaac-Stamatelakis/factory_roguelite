using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;

namespace Conduits.Systems {
    public static class ConduitSystemManagerFactory
    {
        public static IConduitSystemManager CreateManager(
            ConduitType conduitType, 
            Dictionary<Vector2Int,IConduit> conduits,
            Vector2Int size,
            Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts,
            Vector2Int referencePosition
        ) {
            bool isPort = conduitType == ConduitType.Item || conduitType == ConduitType.Fluid || conduitType == ConduitType.Energy || conduitType == ConduitType.Signal;
            if (isPort) {
                return new PortConduitSystemManager(
                    conduitType: conduitType,
                    conduits: CastConduitDict<IPortConduit>(conduits),
                    size: size,
                    chunkConduitPorts: chunkConduitPorts,
                    referencePosition: referencePosition
                );
            }
            bool isMatrix = conduitType == ConduitType.Matrix;
            if (isMatrix) {
                return new MatrixConduitSystemManager(
                    conduitType: conduitType,
                    conduits: CastConduitDict<MatrixConduit>(conduits),
                    size: size,
                    chunkConduitPorts: chunkConduitPorts,
                    referencePosition: referencePosition
                );
            }
            Debug.LogError("ConduitSystemManagerFactory did handle case for " + conduitType);
            return null;
        }

        private static Dictionary<Vector2Int, T> CastConduitDict<T>(Dictionary<Vector2Int,IConduit> conduits)
        {
            Dictionary<Vector2Int, T> dict = new Dictionary<Vector2Int, T>();
            foreach (KeyValuePair<Vector2Int, IConduit> kvp in conduits)
            {
                dict[kvp.Key] = (T)kvp.Value;
            }
            return dict;
        }
    }
}

