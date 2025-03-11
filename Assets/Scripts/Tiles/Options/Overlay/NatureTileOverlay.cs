using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    [CreateAssetMenu(fileName ="New Overlay",menuName="Tile/Overlay/Nature")]
    public class NatureTileOverlay : TileOverlay
    {
        public NatureTile NatureTile;
        public Color Color;
        public override TileBase GetDisplayTile()
        {
            return NatureTile.baseTile;
        }

        public override TileBase GetTile()
        {
            return NatureTile;
        }

        public override Color GetColor()
        {
            return Color;
        }
        
    }
}
