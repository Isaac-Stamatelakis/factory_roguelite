using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using ConduitModule.Ports;
using Newtonsoft.Json;

namespace TileEntityModule.Instances.CompactMachines {
    public class CompactMachineEnergyStorage : TileEntity, IClickableTileEntity, ISerializableTileEntity, IConduitInteractable, IEnergyConduitInteractable
    {
        [SerializeField] public ConduitPortLayout conduitPortLayout;
        [SerializeField] public int storage;
        private int energy;

        public ConduitPortLayout getConduitPortLayout()
        {
            return conduitPortLayout;
        }

        public ref int getEnergy()
        {
            return ref energy;
        }

        public int insertEnergy(int energy)
        {
            throw new System.NotImplementedException();
        }

        public void onClick()
        {
            throw new System.NotImplementedException();
        }

        public string serialize()
        {
            return JsonConvert.SerializeObject(energy);   
        }

        public void unserialize(string data)
        {
            this.energy = JsonConvert.DeserializeObject<int>(data);
        }
    }

}
