using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Storage {
    public class BatteryInstance : TileEntityInstance<Battery>, IRightClickableTileEntity, ISerializableTileEntity, IEnergyConduitInteractable
    {
        public ulong Energy;
        public BatteryInstance(Battery tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }
        
        public void onRightClick()
        {
            TileEntityObject.UIManager.display<BatteryInstance,EnergyStorageUIController>(this);
        }

        public string serialize()
        {
            return Energy.ToString();
        }
        
        public void unserialize(string data)
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

