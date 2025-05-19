using System;
using Chunks;
using Conduits.Ports;
using Dimensions;
using TileEntity.Instances.Storage;
using UnityEngine;
using World.Cave.Registry;

namespace TileEntity.Instances.Caves.DimensionalStabilizer
{
    public class DimensionalStabilizerInstance : TileEntityInstance<DimensionalStabilizerObject>, IEnergyConduitInteractable, IConduitPortTileEntity, IOnCaveRegistryLoadActionTileEntity, IPlaceInitializable, IBreakActionTileEntity, ITickableTileEntity
    {
        private CaveRegistry caveRegistry;
        private ulong lastEnergyInput;
        public DimensionalStabilizerInstance(DimensionalStabilizerObject tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return tileEntityObject.ConduitPortLayout;
        }
        
        public ulong InsertEnergy(ulong energy, Vector2Int portPosition)
        {
            lastEnergyInput = energy;
            
            return energy; // Eats all energy inputed
        }

        private int GetAllotments(ulong energy)
        {
            if (energy == 0) return 0;
            float tier = Mathf.Log(energy, 4);
            int allotments = (int)Mathf.Pow(4, tier);
            return allotments;
        }

        public ulong GetEnergy(Vector2Int portPosition)
        {
            return 0;
        }

        public void SetEnergy(ulong energy, Vector2Int portPosition)
        {
            
        }

        public void OnCaveRegistryLoaded(CaveRegistry registry)
        {
            this.caveRegistry = registry;
            caveRegistry.SetStabilizer(this);
        }

        public void PlaceInitialize()
        {
            caveRegistry = CaveRegistry.Instance;
            caveRegistry.SetStabilizer(this);
        }

        public void OnBreak()
        {
            caveRegistry.RemoveStabilizer();
        }

        public void TickUpdate()
        {
            int allotments = GetAllotments(lastEnergyInput);
            caveRegistry.VerifyAllotments(allotments);
            lastEnergyInput = 0;
        }
    }
}
