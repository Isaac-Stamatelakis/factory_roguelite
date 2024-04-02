using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using TileEntityModule;
using UnityEngine;

namespace ConduitModule.Systems {
    public class PortConduitSystemManager : ConduitSystemManager<IPortConduit,IPortConduitSystem>, ITickableConduitSystem
    {
        public PortConduitSystemManager(ConduitType conduitType, IPortConduit[,] conduits, Vector2Int size, Dictionary<TileEntity, List<TileEntityPort>> chunkConduitPorts, Vector2Int referencePosition) : base(conduitType, conduits, size, chunkConduitPorts, referencePosition)
        {

        }

        public IConduitPort getPort(Vector2Int position) {
            IPortConduit conduit = getConduitCellPosition(position);
            if (conduit == null) {
                return null;
            }
            return conduit.getPort();
        }

        public IPortConduit getConduitWithPort(Vector2Int position) {
            IPortConduit conduit = getConduitCellPosition(position);
            if (conduit == null) {
                return null;
            }
            if (conduit.getPort() == null) {
                return null;
            }
            return conduit;
        }

        public void tickUpdate() {
            foreach (IPortConduitSystem system in conduitSystems) {
                system.tickUpdate();
            }
        }

        public override void onGenerationCompleted()
        {
            // No action
        }
    }
}

