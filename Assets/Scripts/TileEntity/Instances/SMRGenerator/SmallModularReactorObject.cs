using System;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.SMRGenerator
{
    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/SMR")]
    public class SmallModularReactorObject : TileEntityObject
    {
        public ulong EnergyPerTick;
        public ulong MaxEnergy;
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SmallModularReactorInstance(this, tilePosition, tileItem, chunk);
        }
    }

    

    public class SmallModularReactorInstance : TileEntityInstance<SmallModularReactorObject>, IEnergyConduitInteractable, IConduitPortTileEntity, ITickableTileEntity, ISerializableTileEntity
    {
        public ulong Energy;
        public SmallModularReactorInstance(SmallModularReactorObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
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

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            return ref Energy;
        }

        public void TickUpdate()
        {
            if (Energy >= tileEntityObject.MaxEnergy) return;
            Energy += tileEntityObject.EnergyPerTick; // We can allow some overflow cause why not
        }

        public string Serialize()
        {
            return Energy.ToString();
        }

        public void Unserialize(string data)
        {
            Energy = Convert.ToUInt64(data);
        }
    }
}
