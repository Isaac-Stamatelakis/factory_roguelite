using System;
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
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts,
            Vector2Int referencePosition
        ) {
            switch (conduitType)
            {
                case ConduitType.Item:
                case ConduitType.Fluid:
                case ConduitType.Energy:
                case ConduitType.Signal:
                    return new PortConduitSystemManager(
                        conduitType: conduitType,
                        conduits: CastConduitDict<IPortConduit>(conduits),
                        size: size,
                        chunkConduitPorts: chunkConduitPorts,
                        referencePosition: referencePosition
                    );
                case ConduitType.Matrix:
                    return new MatrixConduitSystemManager(
                        conduitType: conduitType,
                        conduits: CastConduitDict<MatrixConduit>(conduits),
                        size: size,
                        chunkConduitPorts: chunkConduitPorts,
                        referencePosition: referencePosition
                    );
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
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

