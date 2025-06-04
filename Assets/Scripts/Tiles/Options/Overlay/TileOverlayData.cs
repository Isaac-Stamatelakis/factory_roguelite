using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    
    public abstract class TileOverlayData : ScriptableObject
    {
        public abstract TileBase GetDisplayTile();
        public abstract TileBase GetTile();
        public abstract Color GetColor();
    }
}
