using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntityModule.Instances.Storage {
    public class BatteryInstance : TileEntityInstance<Battery>, ITickableTileEntity, IRightClickableTileEntity, ISerializableTileEntity, IConduitInteractable, IEnergyConduitInteractable
    {
        private int energy;
        public BatteryInstance(Battery tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public int Energy { get => energy; set => energy = value; }
        public ConduitPortLayout getConduitPortLayout()
        {
            return tileEntity.ConduitPortLayout;
        }

        public ref int getEnergy(Vector2Int portPosition)
        {
            return ref energy;
        }

        public int insertEnergy(int input,Vector2Int portPosition)
        {
            if (Energy >= tileEntity.Storage) {
                return 0;
            }
            int sum = input + Energy;
            if (sum > tileEntity.Storage) {
                Energy = tileEntity.Storage;
                return sum-tileEntity.Storage;
            }
            Energy += input;
            return input;
        }

        public void onRightClick()
        {
            tileEntity.UIManager.display<BatteryInstance,EnergyStorageUIController>(this);
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(Energy);
        }

        public void tickUpdate()
        {
            
        }

        public void unserialize(string data)
        {
            Energy = JsonConvert.DeserializeObject<int>(data);
        }
    }
}

