using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using TileEntity;
using UnityEngine;

namespace Conduits.Systems {
    public class PortConduitSystemManager : ConduitSystemManager<IPortConduit,IPortConduitSystem>, ITickableConduitSystem
    {
        public PortConduitSystemManager(ConduitType conduitType,Dictionary<Vector2Int, IPortConduit> conduits, Vector2Int size, Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts, Vector2Int referencePosition) : base(conduitType, conduits, size, chunkConduitPorts, referencePosition)
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

        public override void onTileEntityAdd(IPortConduit conduit, ITileEntityInstance tileEntity, TileEntityPortData portData)
        {
            Vector2Int relativePosition = tileEntity.getCellPosition() - conduit.GetPosition();
            Debug.Log(relativePosition);
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntity, type);
            conduit.SetPort(ConduitPortFactory.CreateDefault(type,portData.portType,interactable,conduit.GetConduitItem(),relativePosition));
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

