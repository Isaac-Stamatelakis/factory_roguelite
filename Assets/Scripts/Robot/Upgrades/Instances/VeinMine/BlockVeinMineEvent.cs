using TileMaps;
using Tiles;
using UnityEngine;

namespace Robot.Upgrades.Instances.VeinMine
{
    public class BlockVeinMineEvent : VeinMineEvent<WorldTileGridMap>
    {
        private int drillPower;
        private int initialHardness;

        public BlockVeinMineEvent(WorldTileGridMap hitableTileMap, bool drop, int drillPower, int initialHardness) : base(hitableTileMap, drop)
        {
            this.drillPower = drillPower;
            this.initialHardness = initialHardness;
        }

        protected override void InitialExpand(Vector2Int initial)
        {
            Expand(initial);
        }

        protected override bool CanExpandTo(Vector2Int position, Vector2Int origin)
        {
            if (!hitableTileMap.hasTile(position)) return false;
            TileOptions tileOptions = hitableTileMap.getTileItem(position).tileOptions;
            return tileOptions.hardness <= initialHardness && (int) tileOptions.requiredToolTier <= drillPower;
        }
    }
}