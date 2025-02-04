using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Storage {
    
    public class BatteryInstance : TileEntityInstance<Battery>, IRightClickableTileEntity, ISerializableTileEntity, IEnergyConduitInteractable, IConduitPortTileEntity
    {
        public ulong Energy;
        public BatteryInstance(Battery tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }
        
        public void OnRightClick()
        {
            TileEntityObject.UIManager.Display<BatteryInstance,EnergyStorageUIController>(this);
        }

        public string Serialize()
        {
            return Energy.ToString();
        }
        
        public void Unserialize(string data)
        {
            Energy = Convert.ToUInt64(data);
        }

        public ulong InsertEnergy(ulong amount, Vector2Int portPosition)
        {
            ulong maxEnergy = tileEntityObject.Storage;
            if (Energy >= maxEnergy) {
                return 0;
            }
            ulong sum = Energy+=amount;
            if (sum > maxEnergy) {
                Energy = maxEnergy;
                return sum - maxEnergy;
            }
            Energy = sum;
            return amount;
        }

        public ref ulong GetEnergy(Vector2Int portPosition)
        {
            return ref Energy;
        }
    }
}

