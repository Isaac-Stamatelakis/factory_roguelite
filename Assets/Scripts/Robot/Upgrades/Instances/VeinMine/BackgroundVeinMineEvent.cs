using System;
using TileMaps;
using UnityEngine;

namespace Robot.Upgrades.Instances.VeinMine
{
    public class BackGroundVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        public BackGroundVeinMineEvent(WorldTileGridMap hitableTileMap, bool drop, Func<bool> energyCostFunction) : base(hitableTileMap, drop,energyCostFunction)
        {
           
        }
        
        protected override void InitialExpand(Vector2Int initial)
        {
            Expand(initial);
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            return hitableTileMap.HasTile(position);
        }
    }
}