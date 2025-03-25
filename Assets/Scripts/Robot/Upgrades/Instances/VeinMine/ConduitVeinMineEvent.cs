using System;
using Conduit.Conduit;
using Conduits;
using TileMaps.Conduit;
using Tiles;
using UnityEngine;

namespace Robot.Upgrades.Instances.VeinMine
{
    public class ConduitVeinMineEvent : VeinMineEvent<ConduitTileMap>
    {
        private IConduit initialConduit;
        public ConduitVeinMineEvent(ConduitTileMap hitableTileMap, bool drop, Func<bool> energyCostFunction, IConduit initialConduit) : base(hitableTileMap, drop,energyCostFunction)
        {
            this.initialConduit = initialConduit;
        }
        
        protected override void InitialExpand(Vector2Int initial)
        {
            foreach (Vector2Int direction in directions)
            {
                Vector2Int newPosition = initial + direction;
                IConduit conduit = hitableTileMap.ConduitSystemManager.GetConduitAtCellPosition(newPosition);
                if (conduit == null) continue;
                Vector2Int dif = newPosition - initial;
                ConduitDirectionState? conduitDirectionState = ConduitUtils.StateFromVector2Int(dif);
                if (conduitDirectionState == null) continue;
                if (!initialConduit.ConnectsDirection(conduitDirectionState.Value)) continue;
                queue.Enqueue(newPosition);
            }
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            IConduit originConduit = hitableTileMap.ConduitSystemManager.GetConduitAtCellPosition(origin);
            if (originConduit == null) return false;
            IConduit conduit = hitableTileMap.ConduitSystemManager.GetConduitAtCellPosition(position);
            if (conduit == null) return false;
            Vector2Int dif = position-origin;
            ConduitDirectionState? conduitDirectionState = ConduitUtils.StateFromVector2Int(dif);
            if (conduitDirectionState == null) return false;
            return originConduit.ConnectsDirection(conduitDirectionState.Value);
        }
    }
}