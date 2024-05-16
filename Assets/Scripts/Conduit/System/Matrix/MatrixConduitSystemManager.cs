using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using TileEntityModule;
using UnityEngine;

namespace Conduits.Systems {
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

        public override void onTileEntityAdd(MatrixConduit conduit, TileEntity tileEntity, TileEntityPort port)
        {
            IConduitSystem system = conduit.getConduitSystem();
            if (system is not MatrixConduitSystem matrixConduitSystem) {
                Debug.LogError("Matrix conduit did not belong to matrix conduit system");
                return;
            }
            if (tileEntity is not IMatrixConduitInteractable matrixConduitInteractable) {
                return;
            }
            conduit.MatrixConduitInteractable = matrixConduitInteractable;
            matrixConduitSystem.addTileEntityToSystem(conduit,matrixConduitInteractable);
        }

        public override void onTileEntityRemoved(MatrixConduit conduit)
        {
            conduit.MatrixConduitInteractable.removeFromSystem();
            conduit.MatrixConduitInteractable = null;
        }
    }
}

