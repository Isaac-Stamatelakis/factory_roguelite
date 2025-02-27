using TileMaps;
using UnityEngine;

namespace Robot.Upgrades.Instances.VeinMine
{
    public class BackGroundVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        public BackGroundVeinMineEvent(WorldTileGridMap hitableTileMap, bool drop) : base(hitableTileMap, drop)
        {
           
        }

        protected override void InitialExpand(Vector2Int initial)
        {
            Expand(initial);
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            return hitableTileMap.hasTile(position);
        }
    }
}