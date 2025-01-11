using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Highlight
{
    public class TileHighlighter : MonoBehaviour
    {
        [SerializeField] private Tilemap mBaseMap;
        [SerializeField] private SpriteRenderer mOutline;
        private Vector3Int lastCellPosition;
        public void Highlight(Vector2 worldPosition, Tilemap tilemap)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
            cellPosition.z = 0;
            TileBase tileBase = tilemap.GetTile(cellPosition);
            
            if (ReferenceEquals(tileBase, null)) return;
            
            mBaseMap.SetTile(lastCellPosition,null);
            mOutline.transform.position = (Vector2)tilemap.GetCellCenterWorld(cellPosition);
            mBaseMap.SetTile(cellPosition,tileBase);
            lastCellPosition = cellPosition;
            switch (tileBase)
            {
                case Tile tile:
                    mOutline.sprite = tile.sprite;
                    break;
                case AnimatedTile animatedTile when animatedTile.m_AnimatedSprites.Length == 0:
                    return;
                case AnimatedTile animatedTile:
                    mOutline.sprite = animatedTile.m_AnimatedSprites[0];
                    break;
            }
            
        }

        public void Hide()
        {
            mBaseMap.SetTile(lastCellPosition, null);
            mOutline.sprite = null; 
        }
    }
}
