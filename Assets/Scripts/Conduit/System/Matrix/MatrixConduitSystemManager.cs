using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using TileEntityModule;
using UnityEngine;

namespace ConduitModule.Systems {
    public class MatrixConduitSystemManager : ConduitSystemManager<MatrixConduit, MatrixConduitSystem>
    {
        public MatrixConduitSystemManager(ConduitType conduitType, MatrixConduit[,] conduits, Vector2Int size, Dictionary<TileEntity, List<TileEntityPort>> chunkConduitPorts, Vector2Int referencePosition) : base(conduitType, conduits, size, chunkConduitPorts, referencePosition)
        {
        }

        public override void onGenerationCompleted()
        {
            foreach (MatrixConduitSystem system in conduitSystems) {
                system.syncToController();
            }
        }
    }
}

