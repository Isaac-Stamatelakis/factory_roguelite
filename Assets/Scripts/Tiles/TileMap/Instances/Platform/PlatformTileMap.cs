using System;
using System.Collections.Generic;
using Chunks.Partitions;
using Chunks.Systems;
using Dimensions;
using Items;
using Items.Transmutable;
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
    public interface IMultiShaderTilemap
    {
        public void FillShaderList(List<ShaderTilemapManager> managers);
    }
    public class PlatformTileMap : WorldTileMap, IMultiShaderTilemap
    {
        private TileBase[] tileContainer;
        private PlatformSlopeTileMaps leftSlopeMaps;
        private PlatformSlopeTileMaps rightSlopeMaps;
        private Matrix4x4 cachedMatrix;
        private ItemRegistry itemRegistry;
        private ShaderTilemapManager shaderTilemapManager;
        private TileBase emptyTile;
        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            
            tileContainer = new TileBase[3]; // Max 3 tiles placed at once
            MiscDimAssets miscDimAssets = DimensionManager.Instance.MiscDimAssets;
            var slopeColliderExtendTile = miscDimAssets.SlopeExtendColliderTile;
            Material hueShifter = miscDimAssets.HueShifterWorldMaterial;
            emptyTile = miscDimAssets.EmptyTile;
            
            leftSlopeMaps = InitializeSlopeMap(SlopeRotation.Left);
            rightSlopeMaps = InitializeSlopeMap(SlopeRotation.Right);
            itemRegistry = ItemRegistry.GetInstance();
            shaderTilemapManager = new ShaderTilemapManager(transform, -0.1f, false, TileMapType.Platform);
            
            return;
            PlatformSlopeTileMaps InitializeSlopeMap(SlopeRotation rotation)
            {
                var slopeTileMap = AddOverlay(-0.1f);
                slopeTileMap.GetComponent<TilemapRenderer>().material = hueShifter;
                
                slopeTileMap.gameObject.name = "Slope" + rotation;
                slopeTileMap.gameObject.AddComponent<TilemapCollider2D>();
                
                string layerName = "PlatformSlope" + rotation;
                slopeTileMap.gameObject.layer = LayerMask.NameToLayer(layerName);
                TileMapBundleFactory.AddCompositeCollider(slopeTileMap.gameObject,TileMapType.Platform);
                
                var slopeDecoTileMap = AddOverlay(0f);
                slopeDecoTileMap.gameObject.name = "SlopeDecoration"+rotation;
                slopeDecoTileMap.GetComponent<TilemapRenderer>().material = hueShifter;
                
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
            TransmutableItemMaterial transmutableItemMaterial = tileItem.tileOptions.TransmutableColorOverride;
            Material material = !transmutableItemMaterial
                ? null
                : itemRegistry.GetTransmutationWorldMaterial(transmutableItemMaterial);
            switch (platformTileState)
            {
                case PlatformTileState.FlatConnectNone:
                case PlatformTileState.FlatConnectOne:
                case PlatformTileState.FlatConnectAll:
                {
                    TileBase flatTile = tileContainer[0];
                    cachedMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    PlaceFlatTile(flatTile);
                    break;
                }
                case PlatformTileState.SlopeDeco:
                case PlatformTileState.Slope:
                {
                    tilemap.SetTile(vector3Int, emptyTile);
                    TileBase slopeTile = tileContainer[1];
                    TileBase slopeDecoTile = tileContainer[2];
                    PlatformSlopeTileMaps slopeTileMaps = GetSlopeTileMaps(rotation);
                    cachedMatrix.SetTRS(Vector3.zero,Quaternion.Euler(0f, 180*rotation, 0f), Vector3.one);
                    slopeTileMaps.SetTile(ref vector3Int, slopeTile, slopeDecoTile, ref cachedMatrix,ref color, material);
                }
                    break;
                case PlatformTileState.FlatSlopeConnectAllDeco:
                case PlatformTileState.FlatSlopeConnectAll:
                {
                    PlatformSlopeTileMaps slopeTileMaps = GetSlopeTileMaps(rotation);
                    TileBase flatTile = tileContainer[0];
                    TileBase slopeTile = tileContainer[1];
                    TileBase slopeDecoTile = tileContainer[2];
                    PlaceFlatTile(flatTile);
                    slopeTileMaps.SetTile(ref vector3Int, slopeTile, slopeDecoTile, ref cachedMatrix, ref color, material);
                    break;
                }
            }

            void PlaceFlatTile(TileBase tile)
            {
                tilemap.SetTile(vector3Int, tile);
                tilemap.SetTransformMatrix(vector3Int,cachedMatrix);
                if (!material)
                {
                    if (color == Color.white) return;
                    tilemap.SetTileFlags(vector3Int, TileFlags.None);
                    tilemap.SetColor(vector3Int,color);
                    return;
                }
                Tilemap shaderMap = shaderTilemapManager.GetTileMap(material);
                shaderMap.SetTile(vector3Int, tile);
                shaderMap.SetTransformMatrix(vector3Int,cachedMatrix);
                if (color == Color.white) return;
                shaderMap.SetTileFlags(vector3Int, TileFlags.None);
                shaderMap.SetColor(vector3Int, color);
                
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
            Vector3Int cellPosition = new Vector3Int(x, y, 0);
            if (!tilemap.GetTile(cellPosition)) return;
            Vector2Int vector2Int = new Vector2Int(cellPosition.x, cellPosition.y);
            IChunkPartition partition = GetPartitionAtPosition(vector2Int);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(vector2Int);
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            TileItem tileItem = partition.GetTileItem(tilePositionInPartition,TileMapLayer.Base);
            
            var transmutableItem = tileItem.tileOptions.TransmutableColorOverride;
            Material material = !transmutableItem ? null : itemRegistry.GetTransmutationWorldMaterial(transmutableItem);
            tilemap.SetTile(cellPosition,null);
            if (material)
            {
                shaderTilemapManager.GetTileMap(material).SetTile(cellPosition,null);
            }
            
            int state = baseTileData.state;
            bool sloped = state >= (int)PlatformTileState.SlopeDeco;
            
            if (!sloped) return;
            
            if (baseTileData.rotation == 0)
            {
                leftSlopeMaps.Clear(cellPosition,material);
            }
            else
            {
                rightSlopeMaps.Clear(cellPosition,material);
            }
        }

        public override bool HasTile(Vector3Int vector3Int)
        {
            return tilemap.HasTile(vector3Int) || leftSlopeMaps.GetSlopeTileMap().HasTile(vector3Int) ||  rightSlopeMaps.GetSlopeTileMap().HasTile(vector3Int);
        }

        private void UpdateAdjacentTiles(Vector2Int cellPosition)
        {
            UpdateAdjacentTile(Vector2Int.left);
            UpdateAdjacentTile(Vector2Int.left+Vector2Int.down);
            UpdateAdjacentTile(Vector2Int.left+Vector2Int.up);
            
            UpdateAdjacentTile(Vector2Int.right);
            UpdateAdjacentTile(Vector2Int.right+Vector2Int.down);
            UpdateAdjacentTile(Vector2Int.right+Vector2Int.up);
            return;
            void UpdateAdjacentTile(Vector2Int direction)
            {
                Vector2Int adjacentPosition = cellPosition + direction;
                IChunkPartition partition = GetPartitionAtPosition(adjacentPosition);
                if (partition == null) return;
                
                Vector2Int positionInPartition = GetTilePositionInPartition(adjacentPosition);
                BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
                TileItem tileItem = partition.GetTileItem(positionInPartition,TileMapLayer.Base);
                if (!tileItem || tileItem.tileType != TileType.Platform) return;
                
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
            bool valid = platformTileState is PlatformTileState.SlopeDeco or PlatformTileState.FlatSlopeConnectAllDeco;
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
                        return PlatformTileState.SlopeDeco;
                    case PlatformTileState.FlatConnectOne:
                    case PlatformTileState.FlatConnectAll:
                        return PlatformTileState.FlatSlopeConnectAllDeco;
                    case PlatformTileState.SlopeDeco:
                        return PlatformTileState.FlatConnectNone;
                    case PlatformTileState.FlatSlopeConnectAllDeco:
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

        public void FillShaderList(List<ShaderTilemapManager> managers)
        {
            managers.Add(shaderTilemapManager);
            leftSlopeMaps.AddShaperMapsToList(managers);
            rightSlopeMaps.AddShaperMapsToList(managers);
        }
    }
    
    public enum PlatformPlacementMode
    {
        Flat,
        Slope,
        Update
    }
}
