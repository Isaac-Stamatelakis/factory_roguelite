using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using TileEntity;
using UnityEngine;

namespace Conduits.Systems {
    public class PortConduitSystemManager : ConduitSystemManager<IPortConduit,IPortConduitSystem>, ITickableConduitSystem
    {
        public PortConduitSystemManager(ConduitType conduitType,Dictionary<Vector2Int, IPortConduit> conduits, Vector2Int size, Dictionary<ITileEntityInstance, List<TileEntityPort>> chunkConduitPorts, Vector2Int referencePosition) : base(conduitType, conduits, size, chunkConduitPorts, referencePosition)
        {

        }

        public IConduitPort getPort(Vector2Int position) {
            IPortConduit conduit = GetConduitAtRelativeCellPosition(position);
            if (conduit == null) {
                return null;
            }
            return conduit.GetPort();
        }

        public IPortConduit getConduitWithPort(Vector2Int position) {
            IPortConduit conduit = GetConduitAtRelativeCellPosition(position);
            if (conduit == null) {
                return null;
            }
            if (conduit.GetPort() == null) {
                return null;
            }
            return conduit;
        }

        public void tickUpdate() {
            foreach (IPortConduitSystem system in conduitSystems) {
                system.TickUpdate();
            }
        }

        public override void onGenerationCompleted()
        {
            // No action
        }

        public override void onTileEntityAdd(IPortConduit conduit, ITileEntityInstance tileEntity,TileEntityPort port)
        {
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntity, type);
            conduit.SetPort(ConduitPortFactory.CreateDefault(type,port.portType,interactable,conduit.GetConduitItem()));
            conduit.GetConduitSystem().Rebuild();
        }

        public override void onTileEntityRemoved(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.GetPort();
            if (conduitPort == null) {
                return;
            }
            conduit.SetPort(null);
            conduit.GetConduitSystem().Rebuild();
        }
    }
}

