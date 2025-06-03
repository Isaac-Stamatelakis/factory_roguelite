using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Chunks.Systems;
using TileMaps;
using TileMaps.Type;
using Tiles;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using Tiles.TileMap;
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
                LineSearchMode.FirstEmpty => lastHitTile,
                LineSearchMode.FirstHit or LineSearchMode.LastHit => lastHitTile,
                _ => throw new ArgumentOutOfRangeException(nameof(searchMode), searchMode, null)
            };
        }
    }

    public class BackgroundTilePlacementSearcher : BaseTilePlacementSearcher
    {
        private HashSet<Vector2Int> visited = new HashSet<Vector2Int>(128);
        private Queue<Vector2Int> queued = new Queue<Vector2Int>(128);

        private List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.down,
        };
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
            ILoadedChunkSystem chunkSystem = ClosedChunkSystem;
            visited.Clear();
            queued.Clear();
            
            Vector2 playerPosition = PlayerScript.transform.position;
            const int SEARCH_RANGE = 8;
            const float WORLD_SEARCH_RANGE = SEARCH_RANGE * Global.TILE_SIZE;
            float minX = playerPosition.x - WORLD_SEARCH_RANGE;
            float maxX = playerPosition.x + WORLD_SEARCH_RANGE;
            float minY = playerPosition.y - WORLD_SEARCH_RANGE;
            float maxY = playerPosition.y + WORLD_SEARCH_RANGE;
            
            float clampedX = Mathf.Clamp(mousePosition.x,minX,maxX);
            float clampedY = Mathf.Clamp(mousePosition.y,minY,maxY);
            mousePosition = new Vector2(clampedX, clampedY);
            Vector2Int origin = Global.GetCellPositionFromWorld(mousePosition);
            queued.Enqueue(origin);
            Vector2Int bestCandidate = origin;
            while (queued.Count > 0)
            {
                Vector2Int current = queued.Dequeue();
                if (!visited.Add(current)) continue;
                
                TileItem tileItem = GetTileItemAt(current);
                if (tileItem)
                {
                    Vector2Int? candidate = FindBestCandiate(current);
                    if (candidate.HasValue) return CellToVector2(candidate.Value);
                    if (tileItem.tileType == TileType.Block) continue;
                }
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int nextPosition = direction + current;
                    Vector2 nextWorldPosition = CellToVector2(nextPosition);
                    if (!InRange(nextWorldPosition)) continue;
                    queued.Enqueue(nextPosition);
                }
            }
            return CellToVector2(bestCandidate);
            
            bool InRange(Vector2 worldPosition)
            {
                return worldPosition.x >= minX && worldPosition.x <= maxX && worldPosition.y >= minY && worldPosition.y <= maxY;
            }
            Vector2Int? FindBestCandiate(Vector2Int cellPosition)
            {
                Vector2Int? best = null;
                float minCandidateDistance = float.MaxValue;
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int candidate = cellPosition + direction;
                    TileItem tileItem = GetTileItemAt(candidate);
                    if (tileItem?.tileType is TileType.Background) continue;
                    if (tileItem?.tileType is TileType.Block)
                    {
                        var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(candidate);
                        if (partition.GetBaseData(positionInPartition).state == 0) continue;
                    }
                    Vector2 candiateWorld = CellToVector2(candidate);
                    float distance = Vector2.Distance(candiateWorld, mousePosition);
                    if (distance < minCandidateDistance)
                    {
                        minCandidateDistance = distance;
                        best = candidate;
                    }
                }
                return best;
            }

            Vector2 CellToVector2(Vector2Int cell)
            {
                return Global.TILE_SIZE * new Vector2(cell.x + 1 / 2f, cell.y + 1 / 2f);
            }
            
            TileItem GetTileItemAt(Vector2Int cellPosition)
            {
                foreach (IWorldTileMap tileMap in collidableMaps)
                {
                    if (!tileMap.HasTile(cellPosition)) continue;
                    return (TileItem)tileMap.GetItemObject(cellPosition);
                }
                return null;
            }
        }
    }

    public class PlatformTilePlacementSearcher : BaseTilePlacementSearcher
    {
        private readonly List<IWorldTileMap> collidableMaps;
        private readonly PlatformTileMap platformTileMap;
        public PlatformTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem, playerScript)
        {
            platformTileMap = (PlatformTileMap)closedChunkSystem.GetTileMap(TileMapType.Platform);
            collidableMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                platformTileMap
            };
            
        }

        public override Vector2 FindPlacementLocation(Vector2 mousePosition)
        {
            float theta = Mathf.Atan2(mousePosition.y-PlayerScript.transform.position.y, mousePosition.x-PlayerScript.transform.position.x);
            Vector2 mouseDirection = new Vector2(Mathf.Cos(theta),Mathf.Sin(theta));
            BaseTileData autoTileData = PlayerScript.TilePlacementOptions.AutoBaseTileData;
            Vector2? foundTile = TileSearchUtils.BresenhamLine(TileSearchUtils.LineSearchMode.LastHit, PlayerScript.transform.position, mouseDirection,mousePosition,collidableMaps,5f,false);
            if (!foundTile.HasValue) return mousePosition;
            
            if (mouseDirection.y <= -0.2f)
            {
                autoTileData.state = (int)PlatformTileState.FlatConnectNone;
                
                return foundTile.Value + Vector2.right * (mouseDirection.x > 0 ? Global.TILE_SIZE : -Global.TILE_SIZE);
            }
            
            autoTileData.rotation = mouseDirection.x < 0 ? (int)SlopeRotation.Left : (int)SlopeRotation.Right;
            autoTileData.state = (int)PlatformTileState.SlopeDeco;
            return foundTile.Value + Vector2.up * Global.TILE_SIZE;
            
            
        }
    }
}
