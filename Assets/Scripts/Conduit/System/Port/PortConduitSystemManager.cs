using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using TileEntity;
using UnityEngine;

namespace Conduits.Systems {
    public class PortConduitSystemManagerManager : ConduitSystemManager<IPortConduit,IPortConduitSystem>, ITickableConduitSystemManager
    {
        public PortConduitSystemManagerManager(ConduitType conduitType,Dictionary<Vector2Int, IPortConduit> conduits, Dictionary<ITileEntityInstance, List<TileEntityPortData>> chunkConduitPorts) : base(conduitType, conduits, chunkConduitPorts)
        {

        }

        public IConduitPort GetPort(Vector2Int position) {
            IPortConduit conduit = (IPortConduit)GetConduitAtCellPosition(position);
            return conduit?.GetPort();
        }

        public IPortConduit GetConduitWithPort(Vector2Int position) {
            IPortConduit conduit = (IPortConduit)GetConduitAtCellPosition(position);
            return conduit?.GetPort() == null ? null : conduit;
        }

        public void TickUpdate() {
            foreach (IPortConduitSystem system in conduitSystems) {
                system.TickUpdate();
            }
        }

        public List<ITickableConduitSystem> GetTickableConduitSystems()
        {
            List<ITickableConduitSystem> tickableConduitSystems = new List<ITickableConduitSystem>();
            foreach (IPortConduitSystem system in conduitSystems) {
                tickableConduitSystems.Add(system);
            }
            return tickableConduitSystems;
        }

        public override void OnGenerationCompleted()
        {
            // No action
        }

        public override void OnTileEntityAdd(IPortConduit conduit, ITileEntityInstance tileEntity, TileEntityPortData portData)
        {
            IConduitPort port = ConduitPortFactory.CreateDefault(type, portData.portType, tileEntity, conduit.GetConduitItem(), conduit.GetPosition());
            conduit.SetPort(port);
            conduit.GetConduitSystem().Rebuild();
        }

        public override void OnTileEntityRemoved(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.GetPort();
            if (conduitPort == null) {
                return;
            }
            TryDropConduitPortItems(conduit);
            conduit.SetPort(null);
            conduit.GetConduitSystem().Rebuild();
        }
    }
}

