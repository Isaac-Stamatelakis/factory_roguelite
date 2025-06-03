using System;
using Items;
using Items.Transmutable;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    public interface IShaderTileOverlay
    {
        public enum ShaderType
        {
            World,
            UI
        }
        public Material GetMaterial(ShaderType shaderType);
    }
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Overlay/Transmutable")]
    
    public class TransmutableTileOverlayData : TileOverlayData, IShaderTileOverlay
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

        public Material GetMaterial(IShaderTileOverlay.ShaderType shaderType)
        {
            switch (shaderType)
            {
                case IShaderTileOverlay.ShaderType.World:
                    return ItemRegistry.GetInstance().GetTransmutationWorldMaterial(ItemMaterial);
                case IShaderTileOverlay.ShaderType.UI:
                    return ItemRegistry.GetInstance().GetTransmutationUIMaterial(ItemMaterial);
                default:
                    throw new ArgumentOutOfRangeException(nameof(shaderType), shaderType, null);
            }
            
        }
    }
}
