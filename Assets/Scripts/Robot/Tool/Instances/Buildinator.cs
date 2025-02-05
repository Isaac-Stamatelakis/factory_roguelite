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
using TileMaps;
using TileMaps.Layer;
using TileMaps.Type;
using Tiles;
using UnityEngine;


namespace Robot.Tool.Instances
{
    public class Buildinator : RobotToolInstance<BuildinatorData, BuildinatorObject>
    {
        public Buildinator(BuildinatorData toolData, BuildinatorObject robotObject) : base(toolData, robotObject)
        {
         
        }

        public override Sprite GetSprite()
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
            
            
        }

        public override void TerminateClickHold()
        {
            
            
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            if (!Input.GetMouseButtonDown((int)mouseButtonKey)) return; // TODO change this
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            
            IWorldTileMap iWorldTileMap = DimensionManager.Instance.GetPlayerSystem(playerScript.transform).GetTileMap(TileMapType.Block);
            Vector3Int cellPosition = iWorldTileMap.GetTilemap().WorldToCell(mousePosition);
            int direction = mouseButtonKey == MouseButtonKey.Left ? -1 : 1;
            
            switch (toolData.Mode)
            {
                case BuildinatorMode.Chisel:
                    if (iWorldTileMap is not IChiselableTileMap chiselableTileMap) return;
                    chiselableTileMap.IterateChiselTile((Vector2Int)cellPosition, direction);
                    break;
                case BuildinatorMode.Rotator:
                    Rotate(playerScript, cellPosition, direction);
                    break;
                case BuildinatorMode.Hammer:
                    if (iWorldTileMap is not IHammerTileMap stateTile) return;
                    stateTile.IterateHammerTile((Vector2Int)cellPosition, direction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Rotate(PlayerScript playerScript, Vector3Int cellPosition, int direction)
        {
            ClosedChunkSystem system = DimensionManager.Instance.GetPlayerSystem(playerScript.transform);
            IChunkSystem chunkSystem = system;
            var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition((Vector2Int)cellPosition);
            if (partition == null) return;
            TileItem tileItem = partition.GetTileItem(positionInPartition, TileMapLayer.Base);
            if (ReferenceEquals(tileItem, null) || !tileItem.tileOptions.rotatable) return;
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            
            WorldTileGridMap worldTileMap = system.GetTileMap(tileItem.tileType.toTileMapType()) as WorldTileGridMap;
            worldTileMap?.IterateRotatableTile((Vector2Int)cellPosition, direction, baseTileData);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            if (time < 0.125f) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            int direction = moveDirection == MoveDirection.Left ? -1 : 1;
            toolData.Mode = GlobalHelper.ShiftEnum(direction, toolData.Mode);
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
