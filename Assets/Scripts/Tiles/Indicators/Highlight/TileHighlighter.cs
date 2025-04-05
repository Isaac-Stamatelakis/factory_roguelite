using System;
using TileMaps.Place;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Highlight
{
    public class TileHighlighter : MonoBehaviour
    {
        [SerializeField] private Color color;

        [SerializeField] private Tilemap mBaseMap;
        [SerializeField] private SpriteRenderer mOutline;
        private Vector3Int lastCellPosition;
        
        public void Start()
        { 
            int colorKey = Shader.PropertyToID("_Color");
            mOutline.materials[0].SetColor(colorKey,color);
        }
        public void Highlight(Vector2 worldPosition, Tilemap tilemap)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            cellPosition.z = 0;
            TileBase tileBase = tilemap.GetTile(cellPosition);

            mBaseMap.SetTile(lastCellPosition,null);
            if (ReferenceEquals(tileBase, null))
            {
                return;
            }
            
            mBaseMap.SetTile(cellPosition,tileBase);
            lastCellPosition = cellPosition;
            Sprite sprite = GetSprite(tileBase);
            mOutline.sprite = sprite;
            Vector2Int spriteSize = Global.getSpriteSize(sprite);
            Vector2 outlinePosition = (Vector2)tilemap.GetCellCenterWorld(cellPosition);
            if (PlaceTile.mod(spriteSize.x,2) == 0) {
                outlinePosition.x += 0.25f;
            }
            if (PlaceTile.mod(spriteSize.y,2) == 0) {
                outlinePosition.y += 0.25f;
            }

            mOutline.transform.position = outlinePosition;

        }

        private Sprite GetSprite(TileBase tileBase)
        {
            switch (tileBase)
            {
                case Tile tile:
                    return tile.sprite;
                case AnimatedTile animatedTile when animatedTile.m_AnimatedSprites.Length == 0:
                    return null;
                case AnimatedTile animatedTile:
                    return animatedTile.m_AnimatedSprites[0];
                default:
                    throw new System.ArgumentException("Unknown tile type");
            }
        }

        public void Hide()
        {
            mBaseMap.SetTile(lastCellPosition, null);
            mOutline.sprite = null; 
        }
    }
}
