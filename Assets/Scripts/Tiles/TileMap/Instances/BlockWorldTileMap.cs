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
        private ShaderTilemapManager shaderOverlayTilemapManager;
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
            
            shaderOverlayTilemapManager = new ShaderTilemapManager(transform,OVERLAY_Z,false,TileMapType.Block);
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
            Vector2Int tilePosition = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(tilePosition);
            Vector3Int placementPosition = new Vector3Int(x,y,0);
            PlaceTileInTilemap(tilemap,tileItem,placementPosition,partition);
            
            Vector2Int tilePositionInPartition = GetTilePositionInPartition(tilePosition);
            BaseTileData baseTileData = partition.GetBaseData(tilePositionInPartition);
            TileOverlay tileOverlay = tileItem.tileOptions.Overlay;

            var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
            if (transmutableMaterial)
            {
                Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(transmutableMaterial);
                if (material)
                {
                    PlaceTileInTilemap(shaderOverlayTilemapManager.GetTileMap(material), tileItem, placementPosition, partition);
                }
            }
            
            if (tileOverlay)
            {
                Tilemap placementMap = GetOverlayTileMap(tileItem.tileOptions.Overlay);
                PlaceOverlayTile(tileItem.tileOptions.Overlay,placementMap, placementPosition,tileItem,baseTileData);
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
            return new OutlineTileMapCellData(tilemap.GetTile(position),outlineTileMap.GetTile(position),tilemap.GetTransformMatrix(position).rotation,outlineTileMap.GetTransformMatrix(position).rotation,tilemap.GetColor(position));
        }
    }
}

