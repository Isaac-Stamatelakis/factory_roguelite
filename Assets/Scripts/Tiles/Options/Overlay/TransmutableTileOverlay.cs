using Items.Transmutable;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Overlay/Transmutable")]
    public class TransmutableTileOverlay : TileOverlay
    {
        public TileBase TileBase;
        public TransmutableItemMaterial ItemMaterial;
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
            return ItemMaterial?.color ?? Color.white;
        }
    }
}
