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
                if (!outline)
                {
                    Debug.Log("HI");
                    mOutlineTilemap.SetTile(tilePosition,tile);
                    Matrix4x4 matrix4X4 = mOutlineTilemap.GetTransformMatrix(tilePosition);
                    matrix4X4.SetTRS(Vector3.zero,outlineData.OutlineRotation,1.1f * Vector3.one);
                    mOutlineTilemap.SetTransformMatrix(tilePosition,matrix4X4);
                }
                else
                {
                    mOutlineTilemap.SetTile(tilePosition,outline);
                    Matrix4x4 matrix4X4 = mOutlineTilemap.GetTransformMatrix(tilePosition);
                    matrix4X4.SetTRS(Vector3.zero,outlineData.OutlineRotation,Vector3.one);
                    mOutlineTilemap.SetTransformMatrix(tilePosition,matrix4X4);
                }
                
                
                
                
                
            }
        }

        public void Clear()
        {
            mTilemap.ClearAllTiles();
            mOutlineTilemap.ClearAllTiles();
        }
    }
}
