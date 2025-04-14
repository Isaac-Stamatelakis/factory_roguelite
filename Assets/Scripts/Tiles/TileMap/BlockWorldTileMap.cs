using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using TileMaps.Type;
using UnityEngine;
using Tiles;
using Tiles.Options.Overlay;
using UnityEngine.Tilemaps;

namespace TileMaps {
    public struct OutlineTileMapCellData
    {
        public TileBase Tile;
        public TileBase OutlineTile;
        public Quaternion OutlineRotation;
        public Quaternion TileRotation;

        public OutlineTileMapCellData(TileBase tile, TileBase outlineTile, Quaternion tileRotation, Quaternion outlineRotation)
        {
            Tile = tile;
            OutlineTile = outlineTile;
            OutlineRotation = outlineRotation;
            TileRotation = tileRotation;
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
        private ShaderOverlayTilemapManager shaderOverlayTilemapManager;
        public override void Initialize(TileMapType tileMapType)
        {
            base.Initialize(tileMapType);
            
            GameObject outline = new GameObject();
            outline.name = "Outline";
            outline.AddComponent<TilemapRenderer>();
            outlineTileMap = outline.GetComponent<Tilemap>();
            outline.transform.SetParent(transform,false);
            setView(false,Color.black);
            
            GameObject overlayTileMapObject = new GameObject("OverlayTileMap");
            overlayTileMapObject.transform.SetParent(transform,false);
            overlayTileMap = overlayTileMapObject.AddComponent<Tilemap>();
            overlayTileMapObject.AddComponent<TilemapRenderer>();
            overlayTileMapObject.transform.localPosition = new Vector3(0, 0, OVERLAY_Z);
            shaderOverlayTilemapManager = new ShaderOverlayTilemapManager(transform);
        }

        public void setView(bool? wireFrame, Color? color) {
            if (wireFrame != null)
            {
                float z =(bool)wireFrame ? -0.1f : 0.3f;
                outlineTileMap.transform.localPosition = new Vector3(0,0,z);
            }
            
            if (color != null) {
                outlineTileMap.color = (Color)color;
            }
            
        }
        
        


        public override void BreakTile(Vector2Int position)
        {
            base.BreakTile(position);
            Vector3Int vector = new Vector3Int(position.x, position.y, 0);
            outlineTileMap.SetTile(vector, null);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
            shaderOverlayTilemapManager.ClearAllOnTile(ref vector);
        }

        protected override void RemoveTile(int x, int y)
        {
            outlineTileMap.SetTile(new Vector3Int(x,y,0), null);
            Vector3Int vector = new Vector3Int(x,y,0);
            if (overlayTileMap.GetTile(vector)) overlayTileMap.SetTile(vector, null);
            shaderOverlayTilemapManager.ClearAllOnTile(ref vector);
            base.RemoveTile(x, y);
        }

        private Tilemap GetOverlayTileMap(TileOverlay tileOverlay)
        {
            if (tileOverlay is not IShaderTileOverlay shaderTileOverlay) return overlayTileMap;
            Material shaderMaterial = shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World);
            return !shaderMaterial ? overlayTileMap : shaderOverlayTilemapManager.GetTileMap(shaderMaterial);
        }
        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            base.SetTile(x, y, tileItem);
            Vector2Int tilePosition = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(tilePosition);
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(tilePosition);
            if (partition == null) return;
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            
            var tileOverlay = tileItem.tileOptions?.Overlay;
            Vector3Int vec3 = new Vector3Int(x,y,0);
            if (tileOverlay)
            {
                var overlayTile = tileOverlay.GetTile();
                Tilemap placementTilemap = GetOverlayTileMap(tileOverlay);
            
                SetTileItemTile(placementTilemap, overlayTile, vec3, tileItem.tileOptions.rotatable, baseTileData);
                placementTilemap.SetTileFlags(vec3, TileFlags.None); // Required to get color to work
                placementTilemap.SetColor(vec3,tileOverlay.GetColor());
            }
            
            TileBase outlineTile = tileItem.outline;
            if (!outlineTile) {
                return;
            }
            if (outlineTile is not IStateTile stateTile) {
                outlineTileMap.SetTile(new Vector3Int(x,y,0),outlineTile);
                return;
            }
            
            int state = baseTileData.state;

            TileBase stateOutlineTile = stateTile.getTileAtState(state);
            outlineTileMap.SetTile(vec3,stateOutlineTile);

            int rotation = baseTileData.rotation;
            bool mirror = baseTileData.mirror;
            Matrix4x4 transformMatrix = outlineTileMap.GetTransformMatrix(vec3);
            transformMatrix.SetTRS(Vector3.zero, !mirror ? Quaternion.Euler(0f, 0f, 90 * rotation) : Quaternion.Euler(0f, 180f, 90 * rotation), Vector3.one);
            outlineTileMap.SetTransformMatrix(vec3,transformMatrix);
        }

       

        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position)
        {
            return new OutlineTileMapCellData(tilemap.GetTile(position),outlineTileMap.GetTile(position),tilemap.GetTransformMatrix(position).rotation,outlineTileMap.GetTransformMatrix(position).rotation);
        }
    }
}

