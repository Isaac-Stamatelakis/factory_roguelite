using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Chunks.Systems;
using Dimensions;
using Item.ItemObjects.Instances.Tile.Chisel;
using Newtonsoft.Json;
using Player;
using Player.Mouse;
using Player.Tool;
using Player.Tool.Object;
using PlayerModule;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using TileEntity;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Place;
using TileMaps.Type;
using Tiles;
using Tiles.Indicators;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace Robot.Tool.Instances
{
    public interface IClickSpammableTool
    {
        
    }
    public class Buildinator : RobotToolInstance<BuildinatorData, BuildinatorObject>, IDestructiveTool, IAutoSelectTool, IClickSpammableTool
    {
        private RobotToolLaserManager laserManager;
        private int mouseBitMap;
        public Buildinator(BuildinatorData toolData, BuildinatorObject robotObject, RobotStatLoadOutCollection loadOut, PlayerScript playerScript) : base(toolData, robotObject, loadOut, playerScript)
        {
         
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    return robotObject.ChiselSprite;
                case BuildinatorMode.Rotator:
                    return robotObject.RotatorSprite;
                case BuildinatorMode.Hammer:
                    return robotObject.HammerSprite;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public override void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            mouseBitMap |= (int)mouseButtonKey;
            laserManager ??= new RobotToolLaserManager(GameObject.Instantiate(robotObject.LineRendererPrefab, playerScript.transform));
            laserManager.UpdateLineRenderer(mousePosition,GetColor());
        }

        public override void TerminateClickHold(MouseButtonKey mouseButtonKey)
        {
            mouseBitMap &= ~(int)mouseButtonKey;
            playerScript.TileViewers.TileBreakHighlighter.Clear();
            if (laserManager != null && mouseBitMap == 0)
            {
                laserManager.Terminate();
                laserManager = null;
            }
            
            playerScript.PlayerMouse.ClearToolPreview();
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (!Input.GetMouseButton(mouseButtonKey.ToMouseButton())) return;
            
            Vector2 origin = TileHelper.getRealTileCenter(mousePosition);
            
            if (!PlaceTile.raycastTileInBox(origin, TileMapLayer.Base.toRaycastLayers(),true)) return;
            
            Vector2Int vector2Int = Global.getCellPositionFromWorld(mousePosition);
            Vector3Int cellPosition = new Vector3Int(vector2Int.x, vector2Int.y, 0);
            int direction = mouseButtonKey == MouseButtonKey.Left ? -1 : 1;
            
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    Chisel(cellPosition, direction);
                    break;
                case BuildinatorMode.Rotator:
                    Rotate(cellPosition, direction);
                    break;
                case BuildinatorMode.Hammer:
                    Hammer(cellPosition, direction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            playerScript.PlayerMouse.ClearToolPreview();
        }

        public Color GetColor()
        {
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    return new Color(1f, 151f / 255, 0f, 1f);
                case BuildinatorMode.Rotator:
                    return new Color(1f, 151f / 255, 0f, 1f);
                case BuildinatorMode.Hammer:
                    return new Color(1f, 151f / 255, 0f, 1f);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Chisel(Vector3Int vector3Int, int direction)
        {
            IWorldTileMap iWorldTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(TileMapType.Block);
            if (iWorldTileMap is not IChiselableTileMap chiselableTileMap) return;
            Vector2Int cellPosition = (Vector2Int)vector3Int;
            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            for (int x = -multiHits; x <= multiHits; x++)
            {
                for (int y = -multiHits; y <= multiHits; y++)
                {
                    chiselableTileMap.IterateChiselTile(cellPosition + new Vector2Int(x,y), direction);
                }
            }
        }

        private void Hammer(Vector3Int vector3Int, int direction)
        {
            IWorldTileMap iWorldTileMap = DimensionManager.Instance.GetPlayerSystem().GetTileMap(TileMapType.Block);
            if (iWorldTileMap is not IHammerTileMap hammerTileMap) return;
            Vector2Int cellPosition = (Vector2Int)vector3Int;
            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            for (int x = -multiHits; x <= multiHits; x++)
            {
                for (int y = -multiHits; y <= multiHits; y++)
                {
                    hammerTileMap.IterateHammerTile(cellPosition + new Vector2Int(x,y), direction);
                }
            }
        }

        private void Rotate(Vector3Int vector3Int, int direction)
        {
            Vector2Int position = (Vector2Int)vector3Int;
            ClosedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            ILoadedChunkSystem chunkSystem = system;

            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            HashSet<Vector2Int> hitPositions = new HashSet<Vector2Int>();
            List<WorldTileGridMap> worldTileGridMaps = new List<WorldTileGridMap>
            {
                system.GetTileMap(TileMapType.Block) as WorldTileGridMap,
                system.GetTileMap(TileMapType.Object) as WorldTileGridMap
            };
            if (multiHits == 0) // No need for all the fancy stuff below when multi hits are zero
            {
                foreach (WorldTileGridMap worldTileGridMap in worldTileGridMaps)
                {
                    Vector2Int tilePosition = worldTileGridMap.GetHitTilePosition(position);
                    if (!hitPositions.Add(tilePosition)) continue;
                    var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(tilePosition);

                    TileItem tileItem = partition?.GetTileItem(positionInPartition, TileMapLayer.Base);
                
                    if (ReferenceEquals(tileItem, null) || !tileItem.tileOptions.rotatable) continue;

                    RotateTile(tileItem, worldTileGridMap, tilePosition, partition, positionInPartition, direction);
                    break; // Exit after first rotate
                }

                return;
            }
            
            // Sort tiles by distance from the origin
            List<Vector2Int> sortedTiles = new List<Vector2Int>();
            for (int x = -multiHits; x <= multiHits; x++)
            {
                for (int y = -multiHits; y <= multiHits; y++)
                {
                    sortedTiles.Add(new Vector2Int(x, y));
                }
            }
            sortedTiles.Sort((a, b) =>
            {
                int distanceA = Mathf.Abs(a.x) + Mathf.Abs(a.y);
                int distanceB = Mathf.Abs(b.x) + Mathf.Abs(b.y);
                return distanceA.CompareTo(distanceB);
            });

            // Because raycasts are not reliable for simultaenous tile rotations, must store a dict of tiles which have been confirmed as rotatable and then their covered area.
            // If another tile intersects any areas in the rotatable areas, it is not rotated
            Dictionary<Vector2Int, HashSet<Vector2Int>> tilesToRotate = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
            HashSet<Vector2Int> searchedTiles = new HashSet<Vector2Int>();
            
            foreach (Vector2Int sortedTilePosition in sortedTiles)
            {
                Vector2Int cellPosition = position + sortedTilePosition;
                foreach (WorldTileGridMap worldTileGridMap in worldTileGridMaps)
                {
                    Vector2Int tilePosition = worldTileGridMap.GetHitTilePosition(cellPosition);
                    if (!searchedTiles.Add(tilePosition)) continue;
                    var (partition, positionInPartition) =
                        chunkSystem.GetPartitionAndPositionAtCellPosition(tilePosition);

                    TileItem tileItem = partition?.GetTileItem(positionInPartition, TileMapLayer.Base);

                    if (ReferenceEquals(tileItem, null) || !tileItem.tileOptions.rotatable) continue;
                    BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
                    int newRotation = Buildinator.CalculateNewRotation(baseTileData.rotation, direction);

                    Vector2 worldPosition = worldTileGridMap.GetTilemap().CellToWorld((Vector3Int)tilePosition);

                    FloatIntervalVector exclusion = TileHelper.getRealCoveredArea(worldPosition, Global.getSpriteSize(tileItem.getSprite()), baseTileData.rotation);
                    if (!PlaceTile.BaseTilePlacable(tileItem, worldPosition, system, newRotation, exclusion)) continue;

                    FloatIntervalVector newArea = TileHelper.getRealCoveredArea(worldPosition, Global.getSpriteSize(tileItem.getSprite()), newRotation);
                    IntervalVector intervalVector = FloatIntervalVector.ToCellIntervalVector(newArea);

                    bool Condition(int xv, int yv)
                    {
                        Vector2Int vector = new Vector2Int(xv, yv);
                        foreach (var (otherPosition, otherArea) in tilesToRotate)
                        {
                            if (otherArea.Contains(vector)) return false;
                        }

                        return true;
                    }

                    if (!IntervalVector.IterateCondition(intervalVector, Condition)) continue;

                    tilesToRotate[tilePosition] = new HashSet<Vector2Int>();

                    void AddToContained(int xv, int yv) // YAY functional programming!
                    {
                        tilesToRotate[tilePosition].Add(new Vector2Int(xv, yv));
                    }
                    
                    IntervalVector.Iterate(intervalVector, AddToContained);
                    RotateTile(tileItem, worldTileGridMap, tilePosition, partition, positionInPartition, direction);
                    break;
                }
            }
            playerScript.TileViewers.TileBreakHighlighter.Clear();
        }

        private void RotateTile(TileItem tileItem, WorldTileGridMap worldTileGridMap, Vector2Int tilePosition, IChunkPartition partition, Vector2Int positionInPartition, int direction)
        {
            if (tileItem.tileOptions.placeBreakable)
            {
                worldTileGridMap.BreakAndDropTile(tilePosition, true);
                return;
            }
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            worldTileGridMap.IterateRotatableTile(tilePosition, direction, baseTileData);
        }
        

        public static int CalculateNewRotation(int rotation, int direction)
        {
            const int ROTATION_COUNT = 4;
            int newRotation = ((rotation+direction) % ROTATION_COUNT + ROTATION_COUNT) % ROTATION_COUNT;
            return newRotation;
            
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            laserManager?.UpdateLineRenderer(mousePosition,GetColor());
            if (time < 0.25f) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            int direction = moveDirection == MoveDirection.Left ? -1 : 1;
            toolData.Mode = GlobalHelper.ShiftEnum(direction, toolData.Mode);
        }

        public override string GetModeName()
        {
            return toolData?.Mode.ToString();
        }

        public override void Preview(Vector2Int cellPosition)
        {
            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            TileBreakHighlighter tileBreakHighlighter = playerScript.TileViewers.TileBreakHighlighter;
            if (multiHits == 0)
            {
                tileBreakHighlighter.Clear();
                return;
            }
            
            ClosedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            List<WorldTileGridMap> worldTileGridMaps = new List<WorldTileGridMap>
            {
                system.GetTileMap(TileMapType.Block) as WorldTileGridMap
            };
            
            if (toolData.Mode == BuildinatorMode.Rotator)
            {
                worldTileGridMaps.Add(system.GetTileMap(TileMapType.Object) as WorldTileGridMap);
            }
            
            Dictionary<Vector2Int, OutlineTileMapCellData> tiles = new Dictionary<Vector2Int, OutlineTileMapCellData>();
            for (int x = -multiHits; x <= multiHits; x++)
            {
                for (int y = -multiHits; y <= multiHits; y++)
                {
                    Vector2Int breakPosition = cellPosition + new Vector2Int(x, y);
                    foreach (IWorldTileMap tileGridMap in worldTileGridMaps)
                    {
                        if (!tileGridMap.hasTile(breakPosition)) continue;
                        Vector3Int vector3Int = new Vector3Int(breakPosition.x,breakPosition.y,0);
                        if (tileGridMap is IOutlineTileGridMap outlineTileGridMap)
                        {
                            tiles[breakPosition] = outlineTileGridMap.GetOutlineCellData(vector3Int);
                        }
                        else
                        {
                            Tilemap tilemap = tileGridMap.GetTilemap();
                            Quaternion quaternion = tilemap.GetTransformMatrix(vector3Int).rotation;
                            tiles[breakPosition] = new OutlineTileMapCellData(tilemap.GetTile(vector3Int), null,quaternion,quaternion);
                        }
                        
                    }
                    
                }
            }
            tileBreakHighlighter.Display(tiles);
        }
    }

    public enum BuildinatorMode
    {
        Chisel,
        Rotator,
        Hammer
    }

    public class BuildinatorData : RobotToolData
    {
        public BuildinatorMode Mode;
      
        public BuildinatorData(BuildinatorMode mode)
        {
            Mode = mode;
        }
    }
}
