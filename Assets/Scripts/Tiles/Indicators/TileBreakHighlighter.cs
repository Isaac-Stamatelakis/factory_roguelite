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

        public void Display(Dictionary<Vector2Int, OutlineTileMapCellData> tileDict)
        {
            Clear();
            foreach (var (position, outlineData) in tileDict)
            {
                TileBase tile = outlineData.Tile;
                TileBase outline = outlineData.OutlineTile;
                if (!tile) continue;
                
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
        }

        public void Clear()
        {
            mTilemap.ClearAllTiles();
            mOutlineTilemap.ClearAllTiles();
        }
    }
}
