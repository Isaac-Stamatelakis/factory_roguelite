using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Storage {
    public class BatteryInstance : TileEntityInstance<Battery>, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitTileEntityAggregator
    {
        public MachineEnergyInventory EnergyInventory;
        public BatteryInstance(Battery tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
        
        public ConduitPortLayout GetConduitPortLayout()
        {
            return TileEntityObject.ConduitPortLayout;
        }

        public IConduitInteractable GetConduitInteractable(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Energy:
                    return EnergyInventory;
                default:
                    return null;
            }
        }


        public void onRightClick()
        {
            TileEntityObject.UIManager.display<BatteryInstance,EnergyStorageUIController>(this);
        }

        public string serialize()
        {
            return MachineInventoryFactory.SerializedEnergyMachineInventory(EnergyInventory);
        }

        public void tickUpdate()
        {
            
        }

        public void unserialize(string data)
        {
            EnergyInventory = MachineInventoryFactory.DeserializeEnergyMachineInventory(data, this);
        }
    }
}

