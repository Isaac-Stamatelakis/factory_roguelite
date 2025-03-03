using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Indicators
{
    public class TileBreakHighlighter : MonoBehaviour
    {
        [SerializeField] private Tilemap mOutlineTilemap;
        [SerializeField] private Tilemap mTilemap;

        public void Display(Dictionary<Vector2Int, (TileBase, TileBase)> tileDict)
        {
            mTilemap.ClearAllTiles();
            mOutlineTilemap.ClearAllTiles();
            foreach (var (position, pair) in tileDict)
            {
                TileBase tile = pair.Item1;
                TileBase outline = pair.Item2;
                if (!tile || !outline) continue;
                
                Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
                mTilemap.SetTile(tilePosition,tile);
                mOutlineTilemap.SetTile(tilePosition,outline);
            }
        }
    }
}
