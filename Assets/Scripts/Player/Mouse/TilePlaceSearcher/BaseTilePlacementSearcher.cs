using System;
using System.Collections.Generic;
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
        private List<IWorldTileMap> collidableMaps;
        private BackgroundWorldTileMap backgroundTilemap;
        public BackgroundBaseTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem,playerScript)
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
            Vector2? foundTile = CheckCellsWithBresenham(PlayerScript.transform.position, mouseDirection);
            
            if (!foundTile.HasValue) return mousePosition;
           
            return foundTile.Value;
            
            Vector2? CheckCellsWithBresenham(Vector2 origin, Vector2 direction)
            {
                Vector2? lastEmptyTile = null;
                Vector2? foundTile = null;
                float distance = Mathf.Min((origin - mousePosition).magnitude, 5);
                
                Tilemap tilemap = collidableMaps[0].GetTilemap(); // Doesn't matter what map it is all are the same for getting cell
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
                    foreach (IWorldTileMap worldTileMap in collidableMaps)
                    {
                        if (worldTileMap.HasTile(currentCell))
                        {
                            foundTile = new Vector2(currentCell.x * Global.TILE_SIZE, currentCell.y * Global.TILE_SIZE);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
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
