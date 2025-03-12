using Items.Transmutable;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Overlay/Transmutable")]
    public class TransmutableTileOverlay : TileOverlay
    {
        public TileWrapperObject OverlayWrapper;
        public TransmutableItemMaterial ItemMaterial;
        public override TileBase GetDisplayTile()
        {
            return OverlayWrapper?.TileBase;
        }

        public override TileBase GetTile()
        {
            return OverlayWrapper?.TileBase;
        }

        public override Color GetColor()
        {
            return ItemMaterial?.color ?? Color.white;
        }
    }
}
