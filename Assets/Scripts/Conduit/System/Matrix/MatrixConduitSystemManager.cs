using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using TileEntity;
using UnityEngine;

namespace Conduits.Systems {
    public class MatrixConduitSystemManager : ConduitSystemManager<MatrixConduit, MatrixConduitSystem>
    {
        public MatrixConduitSystemManager(ConduitType conduitType, Dictionary<Vector2Int, MatrixConduit> conduits, Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts) : base(conduitType, conduits, chunkConduitPorts)
        {
        }

        public override void OnGenerationCompleted()
        {
            foreach (MatrixConduitSystem system in conduitSystems) {
                system.syncToController();
            }
        }

        public override void OnTileEntityAdd(MatrixConduit conduit, ITileEntityInstance tileEntity, TileEntityPortData portData)
        {
            IConduitSystem system = conduit.GetConduitSystem();
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

        public override void OnTileEntityRemoved(MatrixConduit conduit)
        {
            conduit.MatrixConduitInteractable.RemoveFromSystem();
            conduit.MatrixConduitInteractable = null;
        }
    }
}

