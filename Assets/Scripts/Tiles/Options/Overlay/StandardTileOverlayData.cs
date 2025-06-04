using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Overlay/Standard")]
    public class StandardTileOverlayData : TileOverlayData
    {
        public TileBase TileBase;
        public Color Color;
        public override TileBase GetDisplayTile()
        {
            return TileBase;
        }

        public override TileBase GetTile()
        {
            return TileBase;
        }

        public override Color GetColor()
        {
            return Color;
        }
    }
}
