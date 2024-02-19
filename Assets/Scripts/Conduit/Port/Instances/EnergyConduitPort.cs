using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class EnergyConduitInputPort : IConduitInputPort<int>
    {
        public int color;
        public int priority;
        private int inventory;
        
        private IEnergyConduitInteractable tileEntity;
        [JsonIgnore]
        public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public void insert(int itemSlot) {
            
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

    }

    [System.Serializable]
    public class EnergyConduitOutputPort : IConduitOutputPort<int>
    { 
        public int color;
        public bool roundRobin;
        private int roundRobinIndex;
        private IEnergyConduitInteractable tileEntity;
        [JsonIgnore]
        public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public int extract() {
            
            return 0;
        }

        public void removeTileEntity()
        {
            tileEntity = null;
        }
    }

    public class EnergyConduitPort : ConduitPort<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitPort(EnergyConduitInputPort inPort, EnergyConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
