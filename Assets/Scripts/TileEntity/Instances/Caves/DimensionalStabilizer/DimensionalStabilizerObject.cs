using Chunks;
using Conduits.Ports;
using UnityEngine;
using World.Cave.Registry;

namespace TileEntity.Instances.Caves.DimensionalStabilizer
{
    public interface IConditionalPlacementTileEntityObject
    {
        public bool CanPlace();
    }
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/Cave/Stabilizer")]
    public class DimensionalStabilizerObject : TileEntityObject, IConditionalPlacementTileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new DimensionalStabilizerInstance(this, tilePosition, tileItem, chunk);
        }

        public bool CanPlace()
        {
            return !CaveRegistry.Instance.HasActiveStabilizer();
        }
    }
}
