using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Storage {

    public class CallBackEnergyInventory : EnergyInventory
    {
        private readonly Action onEnergyInsertCallBack;
        public CallBackEnergyInventory(ulong energy, ulong storage, Action onEnergyInsertCallBack) : base(energy, storage)
        {
            this.onEnergyInsertCallBack = onEnergyInsertCallBack;
        }

        public override ulong InsertEnergy(ulong amount, Vector2Int portPosition)
        {
            if (Energy >= Storage) {
                return 0;
            }
            onEnergyInsertCallBack?.Invoke();
            ulong sum = Energy+=amount;
            if (sum > Storage) {
                Energy = Storage;
                return sum - Storage;
            }
            Energy = sum;
            return amount;
        }
    }
    public class EnergyInventory : IEnergyConduitInteractable
    {
        private Action onEnergyInsertCallBack;
        public EnergyInventory(ulong energy, ulong storage)
        {
            Energy = energy;
            Storage = storage;
           
        }

        public ulong Energy;
        public ulong Storage;
        public virtual ulong InsertEnergy(ulong amount, Vector2Int portPosition)
        {
            if (Energy >= Storage) {
                return 0;
            }
            onEnergyInsertCallBack?.Invoke();
            ulong sum = Energy+=amount;
            if (sum > Storage) {
                Energy = Storage;
                return sum - Storage;
            }
            Energy = sum;
            return amount;
        }
        public ulong GetEnergy(Vector2Int portPosition)
        {
            return Energy;
        }

        public void SetEnergy(ulong energy, Vector2Int portPosition)
        {
            Energy = energy;
        }
        public ulong GetSpace()
        {
            return Storage - Energy;
        }
        public float GetFillPercent()
        {
            return ((float)Energy)/Storage;
        }

        public void Fill()
        {
            Energy = Storage;
        }
    }
    public class BatteryInstance : TileEntityInstance<Battery>, ISerializableTileEntity, IEnergyPortTileEntityAggregator, IPlaceInitializable
    {
        public EnergyInventory EnergyInventory;
        public BatteryInstance(Battery tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }

        public IEnergyConduitInteractable GetEnergyConduitInteractable()
        {
            return EnergyInventory;
        }

        public string Serialize()
        {
            return EnergyInventory.Energy.ToString();
        }
        
        public void Unserialize(string data)
        {
            ulong energy = Convert.ToUInt64(data);
            EnergyInventory = new EnergyInventory(energy, tileEntityObject.Storage);
        }

        public void PlaceInitialize()
        {
            EnergyInventory = new EnergyInventory(0, tileEntityObject.Storage);
        }
    }
}

