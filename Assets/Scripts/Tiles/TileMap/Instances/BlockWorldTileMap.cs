using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using TileMaps.Type;
using UnityEngine;
using Tiles;
using Tiles.Options.Overlay;
using Tiles.TileMap;
using UnityEngine.Tilemaps;

namespace TileMaps {
    public struct OutlineTileMapCellData
    {
        public Color TileColor;
        public TileBase Tile;
        public TileBase OutlineTile;
        public TileBase OverlayTile;
        public TileOverlayData OverlayData;
        public Quaternion OutlineRotation;
        public Quaternion TileRotation;
        public Material Material;

        public OutlineTileMapCellData(TileBase tile, TileBase outlineTile, Quaternion tileRotation, Quaternion outlineRotation, Color tileColor, Material material, TileBase overlayTile, TileOverlayData overlayData)
        {
            Tile = tile;
            OutlineTile = outlineTile;
            OutlineRotation = outlineRotation;
            TileRotation = tileRotation;
            TileColor = tileColor;
            Material = material;
            OverlayTile = overlayTile;
            OverlayData = overlayData;
        }
    }
    public interface IOutlineTileGridMap : IWorldTileMap
    {
        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position);
    }
    public class BlockWorldTileMap : WorldTileMap, IOutlineTileGridMap
    {
        private Tilemap outlineTileMap;
        private Tilemap overlayTileMap;
        public override void Initialize(TileMapType tileMapType)
        {
            base.Initialize(tileMapType);
            overlayTileMap = AddOverlay(OVERLAY_Z);
            GameObject outline = new GameObject();
            outline.name = "Outline";
            outline.tag = "Outline";
            outline.AddComponent<TilemapRenderer>();
            outlineTileMap = outline.GetComponent<Tilemap>();
            outline.transform.SetParent(transform,false);
            SetView(false,Color.black);
            primaryShaderTilemap = closedChunkSystem.PrimaryShaderTilemap;
        }

        public void SetView(bool? wireFrame, Color? color) {
            if (wireFrame != null)
            {
                float z = (bool)wireFrame ? -0.1f : 0.3f;
                outlineTileMap.transform.localPosition = new Vector3(0,0,z);
            }
            if (color != null) {
                outlineTileMap.color = (Color)color;
            }
            
        }
        

        protected override void RemoveTile(int x, int y)
        {
            Vector3Int vector = new Vector3Int(x,y,0);
            if (!tilemap.GetTile(vector)) return;
            outlineTileMap.SetTile(new Vector3Int(x,y,0), null);
            overlayTileMap.SetTile(vector, null);
            base.RemoveTile(x, y);
        }
        
        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            Vector2Int tilePosition = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(tilePosition);
            Vector3Int placementPosition = new Vector3Int(x,y,0);
            PlaceTileInTilemap(tilemap,tileItem,placementPosition,partition);
            
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(tilePosition);
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            TileOverlayData tileOverlayData = tileItem.tileOptions.overlayData;

            var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
            if (transmutableMaterial)
            {
                Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(transmutableMaterial);
                if (material)
                {
                    Tilemap shaderMap = primaryShaderTilemap.GetTilemapForPlacement(tilePosition, material);
                    PlaceTileInTilemap(shaderMap, tileItem, placementPosition, partition);
                }
            }
            
            if (tileOverlayData)
            {
                Tilemap overlayPlacementMap;
                if (tileOverlayData is not IShaderTileOverlay shaderTileOverlay)
                {
                    overlayPlacementMap = overlayTileMap;
                }
                else
                {
                    Material shaderMaterial = shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World);
                    overlayPlacementMap = shaderMaterial ? primaryShaderTilemap.GetTilemapForPlacement(tilePosition, shaderMaterial) : overlayTileMap;
                }
                PlaceOverlayTile(tileItem.tileOptions.overlayData,overlayPlacementMap, placementPosition,tileItem,baseTileData);
            }
            TileBase outlineTile = tileItem.outline;
            if (!outlineTile) {
                return;
            }
            if (outlineTile is not IStateTileSingle stateTile) {
                outlineTileMap.SetTile(new Vector3Int(x,y,0),outlineTile);
                return;
            }
            
            int state = baseTileData.state;

            TileBase stateOutlineTile = stateTile.GetTileAtState(state);
            outlineTileMap.SetTile(placementPosition,stateOutlineTile);

            int rotation = baseTileData.rotation;
            bool mirror = baseTileData.mirror;
            tempMatrix4x4.SetTRS(Vector3.zero, !mirror ? Quaternion.Euler(0f, 0f, 90 * rotation) : Quaternion.Euler(0f, 180f, 90 * rotation), Vector3.one);
            outlineTileMap.SetTransformMatrix(placementPosition,tempMatrix4x4);
        }

       

        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position)
        {
            TileItem tileItem = (TileItem)GetItemObject(new  Vector2Int(position.x, position.y));
            Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterialNullSafe(tileItem?.tileOptions.TransmutableColorOverride);
            
            TileBase overlayTile;
            var overlay = tileItem?.tileOptions.overlayData;
            if (overlay)
            {
                if (overlay is IShaderTileOverlay shaderTileOverlay)
                {
                    Tilemap shaderMap = primaryShaderTilemap.GetTilemap(shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World));
                    overlayTile = shaderMap.GetTile(position);
                }
                else
                {
                    overlayTile = overlayTileMap.GetTile(position);
                }
            }
            else
            {
                overlayTile = null;
            }
           
            return new OutlineTileMapCellData(
                tilemap.GetTile(position),
                outlineTileMap.GetTile(position),
                tilemap.GetTransformMatrix(position).rotation,
                outlineTileMap.GetTransformMatrix(position).rotation,
                tilemap.GetColor(position),
                material,
                overlayTile,
                tileItem?.tileOptions.overlayData
            );
        }
    }
}

