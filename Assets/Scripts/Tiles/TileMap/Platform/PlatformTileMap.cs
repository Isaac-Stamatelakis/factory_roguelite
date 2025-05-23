using System;
using Chunks.Partitions;
using Chunks.Systems;
using Dimensions;
using Items;
using Player;
using TileMaps;
using TileMaps.Layer;
using TileMaps.Place;
using TileMaps.Type;
using Tiles.CustomTiles.StateTiles.Instances.Platform;
using Tiles.Options.Overlay;
using Tiles.TileMap.Platform;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public class PlatformTileMap : WorldTileMap
    {
        private TileBase[] tileContainer;
        private PlatformSlopeTileMaps leftSlopeMaps;
        private PlatformSlopeTileMaps rightSlopeMaps;
        private Matrix4x4 cachedMatrix;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            
            tileContainer = new TileBase[3]; // Max 3 tiles placed at once
            var slopeColliderExtendTile = DimensionManager.Instance.MiscDimAssets.SlopeExtendColliderTile;

            leftSlopeMaps = InitializeSlopeMap(SlopeRotation.Left);
            rightSlopeMaps = InitializeSlopeMap(SlopeRotation.Right);

            return;
            PlatformSlopeTileMaps InitializeSlopeMap(SlopeRotation rotation)
            {
                var slopeTileMap = AddOverlay(-0.1f);
                slopeTileMap.gameObject.name = "Slope" + rotation;
                slopeTileMap.gameObject.AddComponent<TilemapCollider2D>();
                
                string layerName = "PlatformSlope" + rotation;
                slopeTileMap.gameObject.layer = LayerMask.NameToLayer(layerName);
                TileMapBundleFactory.AddCompositeCollider(slopeTileMap.gameObject,TileMapType.Platform);
                
                var slopeDecoTileMap = AddOverlay(0f);
                slopeDecoTileMap.gameObject.name = "SlopeDecoration"+rotation;
                
                var slopeColliderExtendTileMap = AddOverlay(0f);
                slopeColliderExtendTileMap.gameObject.name = "SlopeColliderExtend"+rotation;
                TilemapCollider2D slopeExtendCollider = slopeColliderExtendTileMap.gameObject.AddComponent<TilemapCollider2D>();
                slopeExtendCollider.usedByComposite = true;
                slopeColliderExtendTileMap.transform.SetParent(slopeTileMap.transform);
                
                return new PlatformSlopeTileMaps(slopeTileMap, slopeDecoTileMap, slopeColliderExtendTileMap,slopeColliderExtendTile,rotation);
            }
        }

        public Tilemap GetSlopeTilemap(SlopeRotation rotation)
        {
            return rotation == 0 ? leftSlopeMaps.GetSlopeTileMap() : rightSlopeMaps.GetSlopeTileMap();
        }
        public Tilemap GetSlopeTilemap(int rotation)
        {
            return rotation == 0 ? leftSlopeMaps.GetSlopeTileMap() : rightSlopeMaps.GetSlopeTileMap();
        }

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            if (tileItem.tile is not PlatformStateTile platformStateTile) return;
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            Vector3Int vector3Int = new Vector3Int(position.x,position.y,0);
            
            int state = baseTileData.state;
            PlatformTileState platformTileState = (PlatformTileState)baseTileData.state;
            
            platformStateTile.GetTiles(state,tileContainer);

            int rotation = baseTileData.rotation % 2;
            

            Color color = GetTileColor(tileItem);
            switch (platformTileState)
            {
                case PlatformTileState.FlatConnectNone:
                case PlatformTileState.FlatConnectOne:
                case PlatformTileState.FlatConnectAll:
                {
                    TileBase flatTile = tileContainer[0];
                    tilemap.SetTile(vector3Int, flatTile);
                    if (color != Color.white)
                    {
                        tilemap.SetTileFlags(vector3Int, TileFlags.None);
                        tilemap.SetColor(vector3Int,color);
                    }
                    
                    cachedMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    tilemap.SetTransformMatrix(vector3Int,cachedMatrix);
                    break;
                }
                case PlatformTileState.Slope:
                {
                    TileBase slopeTile = tileContainer[1];
                    TileBase slopeDecoTile = tileContainer[2];
                    PlatformSlopeTileMaps slopeTileMaps = GetSlopeTileMaps(rotation);
                    slopeTileMaps.SetTile(ref vector3Int, slopeTile, slopeDecoTile);
                    if (color != Color.white)
                    {
                        slopeTileMaps.SetColor(ref  vector3Int, ref color);
                    }
                    cachedMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    slopeTileMaps.SetTransformMatrix(ref  vector3Int, ref cachedMatrix);
                }
                    break;
                case PlatformTileState.FlatSlopeConnectAll:
                {
                    PlatformSlopeTileMaps slopeTileMaps = GetSlopeTileMaps(rotation);
                    
                    TileBase flatTile = tileContainer[0];
                    TileBase slopeTile = tileContainer[1];
                    TileBase slopeDecoTile = tileContainer[2];
                    tilemap.SetTile(vector3Int, flatTile);
                    slopeTileMaps.SetTile(ref vector3Int, slopeTile, slopeDecoTile);
                    if (color != Color.white)
                    {
                        tilemap.SetTileFlags(vector3Int, TileFlags.None);
                        tilemap.SetColor(vector3Int,color);
                        slopeTileMaps.SetColor(ref  vector3Int, ref color);
                    }
                    cachedMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    tilemap.SetTransformMatrix(vector3Int,cachedMatrix);
                    slopeTileMaps.SetTransformMatrix(ref  vector3Int, ref cachedMatrix);
                    break;
                }
            }
        }

        private PlatformSlopeTileMaps GetSlopeTileMaps(int rotation)
        {
            return rotation % 2 == 0 ? leftSlopeMaps : rightSlopeMaps;
        }

        public override void PlaceNewTileAtLocation(int x, int y, ItemObject itemObject)
        {
            base.PlaceNewTileAtLocation(x, y, itemObject);
            UpdateAdjacentTiles(new Vector2Int(x, y));
        }

        public override void BreakTile(Vector2Int position)
        {
            base.BreakTile(position);
            UpdateAdjacentTiles(position);
        }

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            leftSlopeMaps.Clear(cellPosition);
            rightSlopeMaps.Clear(cellPosition);
        }

        public override bool HasTile(Vector3Int vector3Int)
        {
            return tilemap.HasTile(vector3Int) || leftSlopeMaps.GetSlopeTileMap().HasTile(vector3Int) ||  rightSlopeMaps.GetSlopeTileMap().HasTile(vector3Int);
        }

        private void UpdateAdjacentTiles(Vector2Int cellPosition)
        {
            UpdateAdjacentTile(Vector2Int.left);
            UpdateAdjacentTile(Vector2Int.right);
            return;
            void UpdateAdjacentTile(Vector2Int direction)
            {
                Vector2Int adjacentPosition = cellPosition + direction;
                IChunkPartition partition = GetPartitionAtPosition(adjacentPosition);
                if (partition == null) return;
                
                Vector2Int positionInPartition = GetTilePositionInPartition(adjacentPosition);
                BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
                TileItem tileItem = partition.GetTileItem(positionInPartition,TileMapLayer.Base);
                if (!tileItem) return;
                
                TilePlacementData tilePlacementData = new TilePlacementData((PlayerTileRotation)baseTileData.rotation, baseTileData.state);
                Vector3Int vector3Int = new Vector3Int(adjacentPosition.x,adjacentPosition.y,0);
                Tilemap leftSlopeMap = GetSlopeTilemap(SlopeRotation.Left);
                Tilemap rightSlopeMap = GetSlopeTilemap(SlopeRotation.Right);
                
                int initialState = baseTileData.state;
                int initialRotation = baseTileData.rotation;
                
                PlatformTileState state = TilePlaceUtils.GetPlacementPlatformState(vector3Int, tilePlacementData,tilemap,leftSlopeMap,rightSlopeMap);
                tilePlacementData.State = (int)state;
                baseTileData.state = (int)state;
                int rotation = TilePlaceUtils.GetPlacementPlatformRotation(vector3Int, tilePlacementData,tilemap,leftSlopeMap,rightSlopeMap);
                baseTileData.rotation = rotation;
                
                if (initialState == baseTileData.state && initialRotation == baseTileData.rotation) return;
                RemoveTile(adjacentPosition.x,adjacentPosition.y);
                SetTile(adjacentPosition.x,adjacentPosition.y,tileItem);
            }
        }

        public override void IterateRotatableTile(Vector2Int position, int direction, BaseTileData baseTileData)
        {
            TileItem tileItem = getTileItem(position);
            if (!tileItem) return;
            PlatformTileState platformTileState = (PlatformTileState)baseTileData.state;
            bool valid = platformTileState is PlatformTileState.Slope or PlatformTileState.FlatSlopeConnectAll;
            if (!valid) return;
            baseTileData.rotation = (baseTileData.rotation + 1) % 2;
            RemoveTile(position.x, position.y);
            SetTile(position.x,position.y,getTileItem(position));
        }

        public override void IterateHammerTile(Vector2Int position, int direction)
        {
            TileItem tileItem = getTileItem(position);
            if (!tileItem || tileItem.tileType != TileType.Platform) return;
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return;
            Vector2Int positionInPartition = GetTilePositionInPartition(position);
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            
            baseTileData.state = (int)GetNewState((PlatformTileState)baseTileData.state);
            RemoveTile(position.x, position.y);
            SetTile(position.x,position.y,tileItem);
            return;
            PlatformTileState GetNewState(PlatformTileState current)
            {
                switch (current)
                {
                    case PlatformTileState.FlatConnectNone:
                        return PlatformTileState.Slope;
                    case PlatformTileState.FlatConnectOne:
                    case PlatformTileState.FlatConnectAll:
                        return PlatformTileState.FlatSlopeConnectAll;
                    case PlatformTileState.Slope:
                        return PlatformTileState.FlatConnectNone;
                    case PlatformTileState.FlatSlopeConnectAll:
                        return PlatformTileState.FlatConnectAll;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            return (Vector2Int)tilemap.WorldToCell(position);
        }
    }
    
    public enum PlatformPlacementMode
    {
        Flat,
        Slope,
        Update
    }
}
