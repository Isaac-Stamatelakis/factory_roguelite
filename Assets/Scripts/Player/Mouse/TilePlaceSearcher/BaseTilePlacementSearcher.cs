using System;
using System.Collections.Generic;
using Chunks.Systems;
using TileMaps;
using TileMaps.Type;
using Tiles;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using Tiles.TileMap.Platform;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Player.Mouse.TilePlaceSearcher
{
    public abstract class BaseTilePlacementSearcher
    {
        protected ClosedChunkSystem ClosedChunkSystem;
        protected PlayerScript PlayerScript;
        protected BaseTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript)
        {
            ClosedChunkSystem = closedChunkSystem;
            PlayerScript = playerScript;
        }

        public abstract Vector2 FindPlacementLocation(Vector2 mousePosition);
        

    }

    public static class TilePlacementSearcherFactory
    {
        public static BaseTilePlacementSearcher GetSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript, TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Block:
                    break;
                case TileType.Background:
                    return new BackgroundTilePlacementSearcher(closedChunkSystem,playerScript);
                case TileType.Object:
                    break;
                case TileType.Platform:
                    return new PlatformTilePlacementSearcher(closedChunkSystem,playerScript);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null);
            }

            return null;
        }
    }

    public static class TileSearchUtils
    {
        public enum LineSearchMode
        {
            FirstEmpty,
            FirstHit,
            LastHit
        }
        public static Vector2? BresenhamLine(LineSearchMode searchMode, Vector2 origin, Vector2 direction, Vector2 mousePosition, List<IWorldTileMap> interactableTilemaps, float maxDistance, bool placeableInPlatforms)
        {
            if (interactableTilemaps.Count == 0) return null;
            
            Vector2? lastEmptyTile = null;
            Vector2? lastHitTile = null;
            float distance = Mathf.Min((origin - mousePosition).magnitude, maxDistance);
            
            Tilemap tilemap = interactableTilemaps[0].GetTilemap(); // Doesn't matter what map it is all are the same for getting cell
            Vector3Int startCell = tilemap.WorldToCell(origin);
            Vector2 endPos = origin + direction * distance;
            Vector3Int endCell = tilemap.WorldToCell(endPos);
            
            int x0 = startCell.x;
            int y0 = startCell.y;
            int x1 = endCell.x;
            int y1 = endCell.y;
            
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                Vector2Int currentCell = new Vector2Int(x0, y0);

                bool found = false;
                foreach (IWorldTileMap worldTileMap in interactableTilemaps)
                {
                    if (worldTileMap.HasTile(currentCell))
                    {
                        lastHitTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);  
                        if (searchMode == LineSearchMode.FirstHit)
                        {
                            return lastEmptyTile;
                        }
                        found = true;
                        if (placeableInPlatforms && worldTileMap.GetTileMapType() == TileMapType.Platform)
                        {
                            lastEmptyTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);
                        }
                        break;
                    }
                }

                if (!found)
                {
                    lastEmptyTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);
                    if (lastHitTile.HasValue && searchMode == LineSearchMode.FirstEmpty)
                    {
                        return lastEmptyTile;
                    }
                }
                
                if (x0 == x1 && y0 == y1) break;
    
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return searchMode switch
            {
                LineSearchMode.FirstEmpty => lastEmptyTile,
                LineSearchMode.FirstHit or LineSearchMode.LastHit => lastHitTile,
                _ => throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null)
            };
        }
    }

    public class BackgroundTilePlacementSearcher : BaseTilePlacementSearcher
    {
        private List<IWorldTileMap> collidableMaps;
        private BackgroundWorldTileMap backgroundTilemap;
        public BackgroundTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem,playerScript)
        {
            collidableMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Background),
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Object),
                closedChunkSystem.GetTileMap(TileMapType.Platform),
            };
        }

        public override Vector2 FindPlacementLocation(Vector2 mousePosition)
        {
            float theta = Mathf.Atan2(mousePosition.y-PlayerScript.transform.position.y, mousePosition.x-PlayerScript.transform.position.x);
            Vector2 mouseDirection = new Vector2(Mathf.Cos(theta),Mathf.Sin(theta));
            
            Vector2? foundTile = TileSearchUtils.BresenhamLine(TileSearchUtils.LineSearchMode.FirstEmpty, PlayerScript.transform.position, mouseDirection,mousePosition,collidableMaps,5f,true);
            
            return foundTile ?? mousePosition;
        }
    }

    public class PlatformTilePlacementSearcher : BaseTilePlacementSearcher
    {
        private List<IWorldTileMap> collidableMaps;
        public PlatformTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem, playerScript)
        {
            collidableMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Platform),
            };
        }

        public override Vector2 FindPlacementLocation(Vector2 mousePosition)
        {
            float theta = Mathf.Atan2(mousePosition.y-PlayerScript.transform.position.y, mousePosition.x-PlayerScript.transform.position.x);
            Vector2 mouseDirection = new Vector2(Mathf.Cos(theta),Mathf.Sin(theta));
            BaseTileData autoTileData = PlayerScript.TilePlacementOptions.AutoBaseTileData;
            
            if (mouseDirection.y <= 0)
            {
                autoTileData.state = (int)PlatformTileState.FlatConnectNone;
                Vector2? foundTile = TileSearchUtils.BresenhamLine(TileSearchUtils.LineSearchMode.LastHit, PlayerScript.transform.position, mouseDirection,mousePosition,collidableMaps,5f,false);
                if (!foundTile.HasValue) return mousePosition;
                return foundTile.Value + Vector2.right * (mouseDirection.x > 0 ? Global.TILE_SIZE : -Global.TILE_SIZE);
            }
            
            autoTileData.rotation = mouseDirection.x < 0 ? (int)SlopeRotation.Left : (int)SlopeRotation.Right;
            autoTileData.state = (int)PlatformTileState.SlopeDeco;
            return mousePosition;
        }
    }
}
