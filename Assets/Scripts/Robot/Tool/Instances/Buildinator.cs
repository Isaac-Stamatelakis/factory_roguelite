using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Chunks.Systems;
using Dimensions;
using Item.ItemObjects.Instances.Tile.Chisel;
using Items;
using Newtonsoft.Json;
using Player;
using Player.Mouse;
using Player.Robot;
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
using Tiles.TileMap.Interval;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


namespace Robot.Tool.Instances
{
    public interface IClickSpammableTool
    {
        
    }
    public class Buildinator : RobotToolInstance<BuildinatorData, BuildinatorObject>, IDestructiveTool, IAutoSelectTool, IClickSpammableTool, IPreviewableTool
    {
        private RobotToolLaserManager laserManager;
        
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


        public override void BeginClickHold(Vector2 mousePosition)
        {
            laserManager = new RobotToolLaserManager(GameObject.Instantiate(robotObject.LineRendererPrefab, playerScript.transform),playerScript);
            laserManager.UpdateLineRenderer(mousePosition, GetColor());
        }

        public override void TerminateClickHold()
        {
            laserManager.Terminate();
            playerScript.TileViewers.tileHighlighter.Clear();
            playerScript.PlayerMouse.ClearToolPreview();
        }

        public override void ClickUpdate(Vector2 mousePosition)
        {
            Vector2 origin = TileHelper.getRealTileCenter(mousePosition);
            int layers = TileMapLayer.Base.ToRaycastLayers();
            
            if (!TilePlaceUtils.RaycastTileInBox(origin, layers,true)) return;
            
            Vector2Int vector2Int = Global.WorldToCell(mousePosition);
            Vector3Int cellPosition = new Vector3Int(vector2Int.x, vector2Int.y, 0);
            int direction = Keyboard.current.ctrlKey.isPressed ? -1 : 1;
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

        public TileMapLayer GetAutoSelectLayer()
        {
            return TileMapLayer.Base;
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
            ClosedChunkSystem system = playerScript.CurrentSystem;
            List<WorldTileMap> worldTileGridMaps = new List<WorldTileMap>
            {
                system.GetTileMap(TileMapType.Block) as WorldTileMap,
                system.GetTileMap(TileMapType.Platform) as WorldTileMap
            };
            Vector2Int cellPosition = (Vector2Int)vector3Int;
            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);

            foreach (WorldTileMap worldTileMap in worldTileGridMaps)
            {
                for (int x = -multiHits; x <= multiHits; x++)
                {
                    for (int y = -multiHits; y <= multiHits; y++)
                    {
                        worldTileMap.IterateHammerTile(cellPosition + new Vector2Int(x,y), direction);
                    }
                }
            }
            
        }

        private void Rotate(Vector3Int vector3Int, int direction)
        {
            Vector2Int position = (Vector2Int)vector3Int;
            ClosedChunkSystem system = playerScript.CurrentSystem;
            ILoadedChunkSystem chunkSystem = system;

            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            
            List<WorldTileMap> worldTileGridMaps = new List<WorldTileMap>
            {
                system.GetTileMap(TileMapType.Block) as WorldTileMap,
                system.GetTileMap(TileMapType.Object) as WorldTileMap,
                system.GetTileMap(TileMapType.Platform) as WorldTileMap
            };
            if (multiHits == 0) // No need for all the fancy stuff below when multi hits are zero
            {
                foreach (WorldTileMap worldTileGridMap in worldTileGridMaps)
                {
                    Vector2Int tilePosition = worldTileGridMap.GetHitTilePosition(position);
                   
                    var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(tilePosition);

                    TileItem tileItem = partition?.GetTileItem(positionInPartition, TileMapLayer.Base);
                    
                    if (!tileItem) continue;
                    if (tileItem.tileType != TileType.Platform && !tileItem.tileOptions.rotatable) continue;
                    
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
                foreach (WorldTileMap worldTileGridMap in worldTileGridMaps)
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

                    FloatIntervalVector exclusion = TileHelper.getRealCoveredArea(worldPosition, Global.GetSpriteSize(tileItem.GetSprite()), baseTileData.rotation);
                    if (!TilePlaceUtils.BaseTilePlaceable(tileItem, worldPosition, system, newRotation, exclusion)) continue;

                    FloatIntervalVector newArea = TileHelper.getRealCoveredArea(worldPosition, Global.GetSpriteSize(tileItem.GetSprite()), newRotation);
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
            playerScript.TileViewers.tileHighlighter.Clear();
        }

        private void RotateTile(TileItem tileItem, WorldTileMap worldTileMap, Vector2Int tilePosition, IChunkPartition partition, Vector2Int positionInPartition, int direction)
        {
            if (tileItem.tileOptions.placeBreakable)
            {
                worldTileMap.BreakAndDropTile(tilePosition, true);
                return;
            }
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            worldTileMap.IterateRotatableTile(tilePosition, direction, baseTileData);
        }
        

        public static int CalculateNewRotation(int rotation, int direction)
        {
            const int ROTATION_COUNT = 4;
            int newRotation = ((rotation+direction) % ROTATION_COUNT + ROTATION_COUNT) % ROTATION_COUNT;
            return newRotation;
            
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, float time)
        {
            laserManager.UpdateLineRenderer(mousePosition,GetColor());
            if (time < 0.25f) return false;
            ClickUpdate(mousePosition);
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

        public bool Preview(Vector2Int cellPosition)
        {
            int multiHits = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)BuildinatorUpgrade.MultiHit);
            TileHighlighter tileHighlighter = playerScript.TileViewers.tileHighlighter;
            if (multiHits == 0)
            {
                return false;
            }
            
            ClosedChunkSystem system = DimensionManager.Instance.GetPlayerSystem();
            List<WorldTileMap> worldTileGridMaps = new List<WorldTileMap>
            {
                system.GetTileMap(TileMapType.Block) as WorldTileMap
            };
            
            if (toolData.Mode == BuildinatorMode.Rotator)
            {
                worldTileGridMaps.Add(system.GetTileMap(TileMapType.Object) as WorldTileMap);
            }
            
            Dictionary<Vector2Int, OutlineTileMapCellData> tiles = new Dictionary<Vector2Int, OutlineTileMapCellData>();
            for (int x = -multiHits; x <= multiHits; x++)
            {
                for (int y = -multiHits; y <= multiHits; y++)
                {
                    Vector2Int breakPosition = cellPosition + new Vector2Int(x, y);
                    foreach (var worldTileMap in worldTileGridMaps)
                    {
                        if (!worldTileMap.HasTile(breakPosition)) continue;
                        Vector3Int vector3Int = new Vector3Int(breakPosition.x,breakPosition.y,0);
                        tiles[breakPosition] = worldTileMap.GetOutlineCellData(vector3Int);
                    }
                    
                }
            }
            tileHighlighter.Display(tiles);
            return true;
        }

        public override RobotArmState GetRobotArmAnimation()
        {
            return RobotArmState.Buildinator;
        }

        public override int GetSubState()
        {
            return (int)toolData.Mode;
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
