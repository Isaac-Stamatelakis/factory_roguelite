using System;
using System.Collections.Generic;
using TileMaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Indicators
{
    public class TileBreakHighlighter : MonoBehaviour
    {
        [SerializeField] private Tilemap mOutlineTilemap;
        [SerializeField] private Tilemap mTilemap;
        [SerializeField] private Color color;
        private bool highlighting = false;
        private Vector2Int lastTilePosition;
        
        public void Start()
        { 
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
                
            Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
            mTilemap.SetTile(tilePosition,tile);
            Matrix4x4 mainMatrix4X4 = mTilemap.GetTransformMatrix(tilePosition);
            mainMatrix4X4.SetTRS(mainMatrix4X4.GetPosition(),outlineData.TileRotation,Vector3.one);
            mTilemap.SetTransformMatrix(tilePosition,mainMatrix4X4);
                
            float outlineScale = outline ? 1f : 1.1f;
            TileBase outlineTile = outline ? outline : tile;
            mOutlineTilemap.SetTile(tilePosition,outlineTile);
            Matrix4x4 matrix4X4 = mTilemap.GetTransformMatrix(tilePosition);
            matrix4X4.SetTRS(matrix4X4.GetPosition(),outlineData.OutlineRotation,outlineScale * Vector3.one);
            mOutlineTilemap.SetTransformMatrix(tilePosition,matrix4X4);
        }

        public void ResetHistory()
        {
            lastTilePosition = Vector2Int.left * int.MaxValue;
        }
        public void Clear()
        {
            if (!highlighting) return;
            mTilemap.ClearAllTiles();
            mOutlineTilemap.ClearAllTiles();
            highlighting = false;
        }
    }
}
