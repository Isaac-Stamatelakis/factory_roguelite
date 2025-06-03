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

        public abstract Vector2? FindPlacementLocation(Vector2 mousePosition);
        

    }

    public class TileSearchResultCacher
    {
        private BaseTilePlacementSearcher tileSearcher;
        private Vector2Int lastTile = Vector2Int.one * int.MaxValue;
        private Vector2? lastSearchResult;

        public Vector2? GetResult()
        {
            return lastSearchResult;
        }
        public void CallSearcher(Vector2 mousePosition)
        {
            Vector2Int tile = Global.WorldToCell(mousePosition);
            if (tile == lastTile) return;
            lastSearchResult = tileSearcher?.FindPlacementLocation(mousePosition);
            lastTile = tile;
        }

        public void SetSearcher(BaseTilePlacementSearcher searcher)
        {
            ClearSearchResult();
            tileSearcher = searcher;
        }

        public void ClearSearchResult()
        {
            lastTile = Vector2Int.one * int.MaxValue;
        }
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
    
    public abstract class BaseBfsTilePlacementSearcher : BaseTilePlacementSearcher
    {
        protected readonly HashSet<Vector2Int> Visited = new HashSet<Vector2Int>(128);
        protected readonly Queue<Vector2Int> Queued = new Queue<Vector2Int>(128);
        protected readonly HashSet<Vector2Int> CheckedCandiates = new HashSet<Vector2Int>(128);
        protected List<IWorldTileMap> CollidableMaps;
        protected List<Vector2Int> Directions;
        protected TileType SearchTileType;

        protected BaseBfsTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem, playerScript)
        {
            
        }

        

        public override Vector2? FindPlacementLocation(Vector2 mousePosition)
        {
            
#if UNITY_EDITOR
            if (CollidableMaps == null) Debug.LogWarning($"Collidable maps must be assigned by child of {nameof(BaseBfsTilePlacementSearcher)}");
            if (Directions == null) Debug.LogWarning($"Directions must be assigned by child of {nameof(BaseBfsTilePlacementSearcher)}");
#endif
            ILoadedChunkSystem chunkSystem = ClosedChunkSystem;
            Visited.Clear();
            Queued.Clear();
            CheckedCandiates.Clear();
            
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
            Vector2Int origin = Global.WorldToCell(mousePosition);
            Queued.Enqueue(origin);
            
            while (Queued.Count > 0)
            {
                Vector2Int current = Queued.Dequeue();
                
                if (!Visited.Add(current)) continue;
                
                TileItem tileItem = GetTileItemAt(current);
                if (tileItem)
                {
                    Vector2Int? candidate = FindBestCandiate(current);
                    if (candidate.HasValue) return CellToVector2(candidate.Value);
                    if (tileItem.tileType == TileType.Block) continue;
                }
                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int nextPosition = direction + current;
                    Vector2 nextWorldPosition = CellToVector2(nextPosition);
                    if (!InRange(nextWorldPosition)) continue;
                    Queued.Enqueue(nextPosition);
                }
            }

            return null;
            
            bool InRange(Vector2 worldPosition)
            {
                return worldPosition.x >= minX && worldPosition.x <= maxX && worldPosition.y >= minY && worldPosition.y <= maxY;
            }
            Vector2Int? FindBestCandiate(Vector2Int cellPosition)
            {
                Vector2Int? best = null;
                float minCandidateDistance = float.MaxValue;
                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int candidate = cellPosition + direction;
                    TileItem tileItem = GetTileItemAt(candidate);
                    if (!CheckedCandiates.Add(candidate)) continue;
                    if (tileItem?.tileType == SearchTileType) continue;
                    if (tileItem?.tileType is TileType.Block)
                    {
                        var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(candidate);
                        if (partition.GetBaseData(positionInPartition).state == 0) continue;
                    }
                    Vector2 candiateWorld = CellToVector2(candidate);
                    
                    if (!ValidCandidate(candiateWorld,tileItem)) continue;
                    
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
                foreach (IWorldTileMap tileMap in CollidableMaps)
                {
                    if (!tileMap.HasTile(cellPosition)) continue;
                    return (TileItem)tileMap.GetItemObject(cellPosition);
                }
                return null;
            }
        }

        protected abstract bool ValidCandidate(Vector2 candidateWorldPosition, TileItem candidateItem);
    }

    public class BackgroundTilePlacementSearcher : BaseBfsTilePlacementSearcher
    {
        private readonly int blockLayer;
        
        public BackgroundTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem,playerScript)
        {
            CollidableMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Background),
                closedChunkSystem.GetTileMap(TileMapType.Block),
                closedChunkSystem.GetTileMap(TileMapType.Object),
                closedChunkSystem.GetTileMap(TileMapType.Platform),
            };
            Directions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            blockLayer = 1 << LayerMask.NameToLayer("Block");
            SearchTileType = TileType.Background;
        }
        
        protected override bool ValidCandidate(Vector2 candidateWorldPosition, TileItem candidateItem)
        {
            Vector2 playerPosition = PlayerScript.transform.position;
            Vector2 directionVector2 = -(playerPosition-candidateWorldPosition).normalized;
            var hit = Physics2D.Raycast(playerPosition,directionVector2,(playerPosition-candidateWorldPosition).magnitude,blockLayer);
            return !hit.collider;
        }
    }

    public class PlatformTilePlacementSearcher : BaseBfsTilePlacementSearcher
    {
      
        private readonly PlatformTileMap platformTileMap;
        public PlatformTilePlacementSearcher(ClosedChunkSystem closedChunkSystem, PlayerScript playerScript) : base(closedChunkSystem, playerScript)
        {
            platformTileMap = (PlatformTileMap)closedChunkSystem.GetTileMap(TileMapType.Platform);
            CollidableMaps = new List<IWorldTileMap>
            {
                closedChunkSystem.GetTileMap(TileMapType.Block),
                platformTileMap
            };
            Directions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            SearchTileType = TileType.Platform;

        }

        public override Vector2? FindPlacementLocation(Vector2 mousePosition)
        {
            BaseTileData autoTileData = PlayerScript.TilePlacementOptions.AutoBaseTileData;
            Vector2 playerPosition =  PlayerScript.transform.position;
            float theta = Mathf.Atan2(mousePosition.y-playerPosition.y, mousePosition.x-playerPosition.x);
            float sin = Mathf.Sin(theta);
            if (Mathf.Abs(theta) < Mathf.PI/4)
            {
                // Flat
                Directions = new List<Vector2Int>{Vector2Int.left,Vector2Int.right};
                float playerY = PlayerScript.transform.position.y;
                mousePosition.y = playerY - Global.TILE_SIZE;
                autoTileData.state = (int)PlatformTileState.FlatConnectNone;;
                autoTileData.rotation = 0;
                Vector2? flatResult = base.FindPlacementLocation(mousePosition);
                if (!flatResult.HasValue) return mousePosition;
                return flatResult;
            }
            // Slopes
            Directions = new List<Vector2Int>
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down,
            };
            
            autoTileData.rotation = mousePosition.x - PlayerScript.transform.position.x < 0 ? (int)SlopeRotation.Left : (int)SlopeRotation.Right;
            autoTileData.state = (int)PlatformTileState.SlopeDeco;
            Vector2? result = base.FindPlacementLocation(mousePosition);
            if (!result.HasValue) return mousePosition;
            Vector2Int cellPosition = Global.WorldToCell(result.Value);
            if (platformTileMap.HasTile(cellPosition + Vector2Int.left))
            {
                Debug.Log("A");
                autoTileData.rotation = (int)SlopeRotation.Right;
                return result.Value + new Vector2(0,1) * Global.TILE_SIZE;
            }
            if (platformTileMap.HasTile(cellPosition + Vector2Int.right))
            {
                Debug.Log("B");
                autoTileData.rotation = (int)SlopeRotation.Left;
                return result.Value + new Vector2(0,1) * Global.TILE_SIZE;
            }
            if (platformTileMap.HasTile(cellPosition + Vector2Int.down))
            {
                int rotation = (playerPosition-result.Value).x > 0 ? 0 : 1;
                int direction = (2 * rotation) - 1;
                Debug.Log("C");
                autoTileData.rotation = rotation;
                return result.Value + new Vector2(direction,0) * Global.TILE_SIZE;
            }
            if (platformTileMap.HasTile(cellPosition + Vector2Int.up))
            {
                int rotation = (playerPosition-result.Value).x > 0 ? 0 : 1;
                int direction = (2 * rotation) - 1;
                Debug.Log("D");
                autoTileData.rotation = rotation;
                return result.Value + new Vector2(direction,0) * Global.TILE_SIZE;
            }
            return result.Value + Vector2.up * Global.TILE_SIZE;
        }

        protected override bool ValidCandidate(Vector2 candidateWorldPosition, TileItem candidateItem)
        {
            return true;
        }
    }
}
