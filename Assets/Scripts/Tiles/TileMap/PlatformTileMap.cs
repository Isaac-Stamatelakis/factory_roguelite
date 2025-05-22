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
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public class PlatformTileMap : WorldTileMap
    {
        private TileBase[] tileContainer;
        private Tilemap slopeTileMap;
        public Tilemap SlopeTileMap =>  slopeTileMap;
        private Tilemap slopeColliderExtendTileMap;
        private Tilemap slopeDecoTileMap;
        private TileBase slopeColliderExtendTile;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            slopeTileMap = AddOverlay(-0.1f);
            slopeTileMap.gameObject.name = "Slope";
            slopeTileMap.gameObject.AddComponent<TilemapCollider2D>();
            slopeTileMap.gameObject.layer = LayerMask.NameToLayer("PlatformSlope");
            TileMapBundleFactory.AddCompositeCollider(slopeTileMap.gameObject,TileMapType.Platform);
            
            slopeDecoTileMap = AddOverlay(0f);
            slopeDecoTileMap.gameObject.name = "SlopeDecoration";
            tileContainer = new TileBase[3]; // Max 3 tiles placed at once
            
            slopeColliderExtendTileMap = AddOverlay(0f);
            slopeColliderExtendTileMap.gameObject.name = "SlopeColliderExtend";
            slopeColliderExtendTileMap.gameObject.AddComponent<TilemapCollider2D>();
            slopeColliderExtendTileMap.gameObject.layer = LayerMask.NameToLayer("PlatformSlope");
            
            slopeColliderExtendTile = DimensionManager.Instance.MiscDimAssets.SlopeExtendColliderTile;
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
            Vector3Int slopeDecoPosition = vector3Int + Vector3Int.down;
            int state = baseTileData.state;
            platformStateTile.GetTiles(state,tileContainer);

            int rotation = baseTileData.rotation % 2;
            TileBase flatTile = tileContainer[0];
            tilemap.SetTile(vector3Int, flatTile);

            TileBase slopeTile = tileContainer[1];
            bool sloped = slopeTile;
            if (sloped)
            {
                slopeTileMap.SetTile(vector3Int, slopeTile);
                
                TileBase slopeDecoTile = tileContainer[2];
                slopeDecoTileMap.SetTile(slopeDecoPosition, slopeDecoTile);
                Vector3Int extendTileDirection = rotation == 0 ? Vector3Int.right : Vector3Int.left;
                slopeColliderExtendTileMap.SetTile(vector3Int+extendTileDirection, slopeColliderExtendTile);
            }
            
            
            Color color = GetTileColor(tileItem);
            if (color != Color.white)
            {
                ColorTileMap(tilemap,vector3Int);
                ColorTileMap(slopeTileMap,vector3Int);
                ColorTileMap(slopeDecoTileMap,slopeDecoPosition);
            }
            
            Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(vector3Int);
            transformMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
            tilemap.SetTransformMatrix(vector3Int,transformMatrix);
            
            if (!sloped) return;
            slopeTileMap.SetTransformMatrix(vector3Int,transformMatrix);
            slopeDecoTileMap.SetTransformMatrix(slopeDecoPosition,transformMatrix);
            slopeColliderExtendTileMap.SetTransformMatrix(vector3Int+Vector3Int.left,transformMatrix);

            return;

            void ColorTileMap(Tilemap map,Vector3Int colorPosition)
            {
                map.SetTileFlags(colorPosition, TileFlags.None);
                map.SetColor(colorPosition,color);
            }
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
            
            if (!slopeTileMap.GetTile(cellPosition)) return;
            
            Matrix4x4 transformMatrix = slopeTileMap.GetTransformMatrix(cellPosition);
            slopeTileMap.SetTile(cellPosition, null);
            slopeDecoTileMap.SetTile(cellPosition+Vector3Int.down, null);

            Vector3Int extendOffset = transformMatrix.rotation == Quaternion.identity ? Vector3Int.right : Vector3Int.left;
            slopeColliderExtendTileMap.SetTile(cellPosition+extendOffset, null); 
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
                TilePlacementData tilePlacementData = new TilePlacementData((PlayerTileRotation)baseTileData.rotation, baseTileData.state, (int)PlatformPlacementMode.Update);
                PlatformTileState state = TilePlaceUtils.GetPlacementPlatformState(adjacentPosition, tilePlacementData,this);
                tilePlacementData.State = (int)state;
                int rotation = TilePlaceUtils.GetPlacementPlatformRotation(adjacentPosition, tilePlacementData,this);
                baseTileData.state = (int)state;
                baseTileData.rotation = rotation;
                SetTile(adjacentPosition.x,adjacentPosition.y,tileItem);

            }
        }

        public override void IterateRotatableTile(Vector2Int position, int direction, BaseTileData baseTileData)
        {
            TileItem tileItem = getTileItem(position);
            if (!tileItem) return;
            PlatformTileState platformTileState = (PlatformTileState)baseTileData.state;
            bool valid = platformTileState is PlatformTileState.Slope or PlatformTileState.FlatSlopeConnectAll or PlatformTileState.FlatSlopeConnectOne;
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
                        return PlatformTileState.FlatSlopeConnectOne;
                    case PlatformTileState.FlatConnectAll:
                        return PlatformTileState.FlatSlopeConnectAll;
                    case PlatformTileState.Slope:
                        return PlatformTileState.FlatConnectNone;
                    case PlatformTileState.FlatSlopeConnectOne:
                        return PlatformTileState.FlatConnectOne;
                    case PlatformTileState.FlatSlopeConnectAll:
                        return PlatformTileState.FlatConnectAll;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public override bool HasTile(Vector3Int vector3Int)
        {
            return tilemap.HasTile(vector3Int) || slopeTileMap.HasTile(vector3Int);
        }

        public bool HasSlopeTile(Vector3Int vector3Int)
        {
            return slopeTileMap.HasTile(vector3Int);
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
