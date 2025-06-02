using System;
using Chunks.Systems;
using TileMaps;
using TileMaps.Type;
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
                    return new BackgroundBaseTilePlacementSearcher(closedChunkSystem,playerScript);
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
        public BackgroundBaseTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem,playerScript)
        {
            backgroundTilemap = (BackgroundWorldTileMap)closedChunkSystem.GetTileMap(TileMapType.Background);
        }

        public override Vector2 FindPlacementLocation(Vector2 mousePosition)
        {
            float theta = Mathf.Atan2(mousePosition.y-PlayerScript.transform.position.y, mousePosition.x-PlayerScript.transform.position.x);
            
            Vector2 mouseDirection = new Vector2(Mathf.Cos(theta),Mathf.Sin(theta));
            Vector2? foundTile = CheckCellsWithBresenham(PlayerScript.transform.position, mouseDirection);
            
            if (!foundTile.HasValue) return mousePosition;
            Vector2Int offset = new Vector2Int(GetOffset(mouseDirection.x), GetOffset(mouseDirection.y));
            const float OFFSET_BOUNDS = 0.2f;

            return foundTile.Value;

            int GetOffset(float direction)
            {
                return direction switch
                {
                    > -OFFSET_BOUNDS => 1,
                    < -OFFSET_BOUNDS => -1,
                    _ => 0
                };
            }
            Vector2? CheckCellsWithBresenham(Vector2 origin, Vector2 direction)
            {
                Vector2? lastEmptyTile = null;
                Vector2? foundTile = null;
                float distance = Mathf.Min((origin - mousePosition).magnitude, 5);
                Tilemap tilemap = backgroundTilemap.GetTilemap();
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
                    Vector3Int currentCell = new Vector3Int(x0, y0, 0);
        
                    if (tilemap.HasTile(currentCell))
                    {
                        foundTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);
                    }
                    else
                    {
                        lastEmptyTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);
                        if (foundTile.HasValue)
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

                return lastEmptyTile;
            }
            
        }
    }
}
