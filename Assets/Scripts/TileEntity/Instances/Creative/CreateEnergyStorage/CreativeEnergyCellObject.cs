using System;
using Chunks;
using Conduits.Ports;
using Item.Slot;
using Items;
using Player;
using TileEntity.Instances.Storage.Fluid;
using UI;
using UnityEngine;

namespace TileEntity.Instances.Creative.CreativeChest
{
    [CreateAssetMenu(fileName = "New Creative Cell", menuName = "Tile Entity/Creative/Cell")]
    public class CreativeEnergyCellObject : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CreativeCellInstance(this, tilePosition, tileItem, chunk);
        }
    }

    public class CreativeCellInstance : TileEntityInstance<CreativeEnergyCellObject>, IEnergyConduitInteractable, IConduitPortTileEntity
    {
        public CreativeCellInstance(CreativeEnergyCellObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }
        
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }
        
        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            return 0;
        }

        public ulong GetEnergy(Vector2Int portPosition)
        {
            return ulong.MaxValue;
        }

        public void SetEnergy(ulong energy, Vector2Int portPosition)
        {
            
        }
    }
}
