using System;
using TileMaps;
using Tiles;
using UnityEngine;

namespace Robot.Upgrades.Instances.VeinMine
{
    public class BlockVeinMineEvent : VeinMineEvent<WorldTileMap>
    {
        private int drillPower;
        private int initialHardness;

        public BlockVeinMineEvent(WorldTileMap hitableTileMap, bool drop, Func<bool> energyCostFunction, int drillPower, int initialHardness) : base(hitableTileMap, drop,energyCostFunction)
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
            if (!hitableTileMap.HasTile(position)) return false;
            TileItem tileItem = hitableTileMap.getTileItem(position);
            if (!tileItem || tileItem.tileType != TileType.Block) return false;
            TileOptions tileOptions = tileItem.tileOptions;
            return tileOptions.hardness <= initialHardness && (int) tileOptions.requiredToolTier <= drillPower;
        }
    }
}