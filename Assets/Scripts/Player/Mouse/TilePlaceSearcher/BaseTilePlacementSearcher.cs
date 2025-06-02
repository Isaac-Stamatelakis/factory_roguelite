using System;
using Chunks.Systems;
using TileMaps;
using TileMaps.Type;
using UnityEngine;

namespace Player.Mouse.TilePlaceSearcher
{
    public abstract class BaseTilePlacementSearcher
    {
        protected ClosedChunkSystem ClosedChunkSystem;

        protected BaseTilePlacementSearcher(ClosedChunkSystem closedChunkSystem)
        {
            ClosedChunkSystem = closedChunkSystem;
        }

        public abstract Vector2 FindPlacementLocation(Vector2 worldPosition);
    }

    public static class TilePlacementSearcherFactory
    {
        public static BaseTilePlacementSearcher GetSearcher(ClosedChunkSystem closedChunkSystem, TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Block:
                    break;
                case TileType.Background:
                    return new BackgroundBaseTilePlacementSearcher(closedChunkSystem);
                case TileType.Object:
                    break;
                case TileType.Platform:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
            }

            return null;
        }
    }

    public class BackgroundBaseTilePlacementSearcher : BaseTilePlacementSearcher
    {
        private BackgroundWorldTileMap backgroundTilemap;
        public BackgroundBaseTilePlacementSearcher(ClosedChunkSystem closedChunkSystem) : base(closedChunkSystem)
        {
            backgroundTilemap = (BackgroundWorldTileMap)closedChunkSystem.GetTileMap(TileMapType.Background);
        }

        public override Vector2 FindPlacementLocation(Vector2 worldPosition)
        {
            return worldPosition;
        }
    }
}
