using Chunks;
using Dimensions;
using UnityEngine;

namespace TileEntity.Instances.Caves.ReturnPortal
{
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/Cave/Return")]
    public class ReturnPortalObject : TileEntityObject
    {
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ReturnPortalInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class ReturnPortalInstance : TileEntityInstance<ReturnPortalObject>, IRightClickableTileEntity, ILoadableTileEntity
    {
        private GameObject lightObject;
        public ReturnPortalInstance(ReturnPortalObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            DimensionManager.Instance.SetPlayerSystem(PlayerManager.Instance.GetPlayer(), Dimension.OverWorld, Vector2.zero);
        }

        public void Load()
        {
            if (lightObject || chunk is not ILoadedChunk loadedChunk) return;
            lightObject = TileEntityUtils.SpawnLightObject(Color.white, 1f, 7f,
            0.7f,
            GetWorldPositionInChunk(), loadedChunk.GetTileEntityContainer());
        }

        public void Unload()
        {
            if (!lightObject) return;
            Object.Destroy(lightObject);
        }
    }
}
