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
        public Color TileColor;
        public TileBase Tile;
        public TileBase OutlineTile;
        public Quaternion OutlineRotation;
        public Quaternion TileRotation;

        public OutlineTileMapCellData(TileBase tile, TileBase outlineTile, Quaternion tileRotation, Quaternion outlineRotation, Color tileColor)
        {
            Tile = tile;
            OutlineTile = outlineTile;
            OutlineRotation = outlineRotation;
            TileRotation = tileRotation;
            TileColor = tileColor;
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
            overlayTileMap = AddOverlay(OVERLAY_Z);
            GameObject outline = new GameObject();
            outline.name = "Outline";
            outline.AddComponent<TilemapRenderer>();
            outlineTileMap = outline.GetComponent<Tilemap>();
            outline.transform.SetParent(transform,false);
            setView(false,Color.black);
            
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
        

        protected override void RemoveTile(int x, int y)
        {
            Vector3Int vector = new Vector3Int(x,y,0);
            if (!tilemap.GetTile(vector)) return;
            outlineTileMap.SetTile(new Vector3Int(x,y,0), null);
            overlayTileMap.SetTile(vector, null);
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
            
            Vector3Int vec3 = new Vector3Int(x,y,0);
            TileOverlay tileOverlay = tileItem.tileOptions.Overlay;
            if (tileOverlay)
            {
                Tilemap placementMap = GetOverlayTileMap(tileItem.tileOptions.Overlay);
                PlaceOverlayTile(tileItem.tileOptions.Overlay,placementMap, vec3,tileItem,baseTileData);
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
            outlineTileMap.SetTile(vec3,stateOutlineTile);

            int rotation = baseTileData.rotation;
            bool mirror = baseTileData.mirror;
            Matrix4x4 transformMatrix = outlineTileMap.GetTransformMatrix(vec3);
            transformMatrix.SetTRS(Vector3.zero, !mirror ? Quaternion.Euler(0f, 0f, 90 * rotation) : Quaternion.Euler(0f, 180f, 90 * rotation), Vector3.one);
            outlineTileMap.SetTransformMatrix(vec3,transformMatrix);
        }

       

        public OutlineTileMapCellData GetOutlineCellData(Vector3Int position)
        {
            return new OutlineTileMapCellData(tilemap.GetTile(position),outlineTileMap.GetTile(position),tilemap.GetTransformMatrix(position).rotation,outlineTileMap.GetTransformMatrix(position).rotation,tilemap.GetColor(position));
        }
    }
}

