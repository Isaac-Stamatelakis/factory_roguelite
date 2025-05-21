using System;
using Chunks.Partitions;
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
        private Tilemap slopeDecoTileMap;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            slopeTileMap = AddOverlay();
            slopeDecoTileMap = AddOverlay();
            tileContainer = new TileBase[3]; // Max 3 tiles placed at once
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
            tilemap.SetTile(vector3Int, tileContainer[0]);
            slopeTileMap.SetTile(vector3Int, tileContainer[1]);
            slopeDecoTileMap.SetTile(slopeDecoPosition, tileContainer[2]);
            
            Color color = GetTileColor(tileItem);
            if (color != Color.white)
            {
                ColorTileMap(tilemap);
                ColorTileMap(slopeTileMap);
                ColorTileMap(slopeDecoTileMap);
            }
            
            if (rotation == 1)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(vector3Int);
                transformMatrix.SetTRS(Vector3.zero,  Quaternion.Euler(0f, 180f, 0f), Vector3.one);
                tilemap.SetTransformMatrix(vector3Int,transformMatrix);
                slopeTileMap.SetTransformMatrix(vector3Int,transformMatrix);
                slopeDecoTileMap.SetTransformMatrix(slopeDecoPosition,transformMatrix);
            }
            
            return;

            void ColorTileMap(Tilemap map)
            {
                map.SetTileFlags(vector3Int, TileFlags.None);
                map.SetColor(vector3Int,color);
            }
        }

        public override void PlaceNewTileAtLocation(int x, int y, ItemObject itemObject)
        {
            base.PlaceNewTileAtLocation(x, y, itemObject);
            UpdateAdjacentTiles(new Vector2Int(x, y));
        }

        protected override void RemoveTile(int x, int y)
        {
            base.RemoveTile(x, y);
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            slopeTileMap.SetTile(cellPosition, null);
            slopeDecoTileMap.SetTile(cellPosition, null);
            UpdateAdjacentTiles(new Vector2Int(x, y));
        }
        
        
        private void UpdateAdjacentTiles(Vector2Int cellPosition)
        {
            UpdateAdjacentTile(cellPosition + Vector2Int.left);
            UpdateAdjacentTile(cellPosition + Vector2Int.right);
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
                Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
                TilePlacementData tilePlacementData = new TilePlacementData((PlayerTileRotation)baseTileData.rotation, baseTileData.state, (int)PlatformPlacementMode.Update);
                Vector2 worldPosition = tilemap.CellToWorld(vector3Int);
                PlatformTileState state = TilePlaceUtils.GetPlacementPlatformState(worldPosition, tilePlacementData);
                int rotation = TilePlaceUtils.GetPlacementPlatformRotation(worldPosition, tilePlacementData);
                baseTileData.state = (int)state;
                baseTileData.rotation = rotation;
                SetTile(adjacentPosition.x,adjacentPosition.y,tileItem);

            }
        }
        
    }
    
    public enum PlatformPlacementMode
    {
        Flat,
        Slope,
        Update
    }
}
