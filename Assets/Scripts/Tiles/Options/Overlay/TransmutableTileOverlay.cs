using Items.Transmutable;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    public interface IShaderTileOverlay
    {
        public Material GetMaterial();
    }
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Overlay/Transmutable")]
    
    public class TransmutableTileOverlay : TileOverlay, IShaderTileOverlay
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

        public Material GetMaterial()
        {
            return ItemMaterial.ShaderMaterial;
        }
    }
}
