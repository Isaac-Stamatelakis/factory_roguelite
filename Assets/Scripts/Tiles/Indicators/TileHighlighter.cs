using System;
using System.Collections.Generic;
using TileMaps;
using TileMaps.Type;
using Tiles.Options.Overlay;
using Tiles.TileMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Indicators
{
    public class TileHighlighter : MonoBehaviour
    {
        [SerializeField] private Tilemap mOutlineTilemap;
        [SerializeField] private Tilemap mTilemap;
        [SerializeField] private Tilemap mOverlayTilemap;
        [SerializeField] private Color color;
        
        private ShaderTilemapManager shaderTilemapManager;
        private bool highlighting = false;
        private Vector2Int lastTilePosition;
        
        public void Start()
        {
            shaderTilemapManager = new ShaderTilemapManager(mTilemap.transform, -3, false, TileMapType.Block);
            SetOutlineColor(color);
        }

        public void SetOutlineColor(Color outlineColor)
        {
            int colorKey = Shader.PropertyToID("_Color");
            mOutlineTilemap.GetComponent<TilemapRenderer>().materials[0].SetColor(colorKey,outlineColor);
        }
        public void Display(Dictionary<Vector2Int, OutlineTileMapCellData> tileDict)
        {
            Clear();
            highlighting = true;
            foreach (var (position, outlineData) in tileDict)
            {
                DisplayTile(position, outlineData);
            }
        }

        public void Display(Vector2Int position, OutlineTileMapCellData outlineData)
        {
            if (position == lastTilePosition) return;
            lastTilePosition = position;
            Clear();
            highlighting = true;
            DisplayTile(position, outlineData);
        }

        private void DisplayTile(Vector2Int position, OutlineTileMapCellData outlineData)
        {
            TileBase tile = outlineData.Tile;
            TileBase outline = outlineData.OutlineTile;
            if (!tile) return;
            mTilemap.gameObject.SetActive(true);
            mOutlineTilemap.gameObject.SetActive(true);
            mOverlayTilemap.gameObject.SetActive(true);
            
            Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);

            Tilemap placementMap = outlineData.Material ? shaderTilemapManager.GetTileMap(outlineData.Material) : mTilemap;
            placementMap.SetTile(tilePosition,tile);
            Matrix4x4 mainMatrix4X4 = placementMap.GetTransformMatrix(tilePosition);
            mainMatrix4X4.SetTRS(mainMatrix4X4.GetPosition(),outlineData.TileRotation,Vector3.one);
            placementMap.SetTransformMatrix(tilePosition,mainMatrix4X4);
            if (outlineData.TileColor != Color.white)
            {
                placementMap.SetTileFlags(tilePosition,TileFlags.None);
                placementMap.SetColor(tilePosition,outlineData.TileColor);
            }
            
            float outlineScale = outline ? 1f : 1.1f;
            TileBase outlineTile = outline ? outline : tile;
            mOutlineTilemap.SetTile(tilePosition,outlineTile);
            Matrix4x4 matrix4X4 = mOutlineTilemap.GetTransformMatrix(tilePosition);
            matrix4X4.SetTRS(matrix4X4.GetPosition(),outlineData.OutlineRotation,outlineScale * Vector3.one);
            mOutlineTilemap.SetTransformMatrix(tilePosition,matrix4X4);
            
            var overlay = outlineData.Overlay;
            if (overlay)
            {
                Material overlayMaterial = overlay is IShaderTileOverlay shaderTileOverlay
                    ? shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World)
                    : null;
                Tilemap overlayPlacementMap = !overlayMaterial ? mOverlayTilemap : shaderTilemapManager.GetTileMap(overlayMaterial);
                overlayPlacementMap.SetTile(tilePosition,outlineData.OverlayTile);
                overlayPlacementMap.SetTransformMatrix(tilePosition,matrix4X4);
                overlayPlacementMap.SetTileFlags(tilePosition,TileFlags.None);
                overlayPlacementMap.SetColor(tilePosition,overlay.GetColor());
            }
        }

        public void ResetHistory()
        {
            lastTilePosition = Vector2Int.left * int.MaxValue;
        }
        public void Clear()
        {
            if (!highlighting) return;
            ResetHistory();
            mTilemap.ClearAllTiles();
            mOutlineTilemap.ClearAllTiles();
            shaderTilemapManager.ClearAllTiles();
            shaderTilemapManager.PushUnusedMaps();
            mOverlayTilemap.ClearAllTiles();
            
            mTilemap.gameObject.SetActive(false);
            mOutlineTilemap.gameObject.SetActive(false);
            mOverlayTilemap.gameObject.SetActive(false);
            
            highlighting = false;
        }
    }
}
