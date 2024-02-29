using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class EnergyConduitInputPort : IConduitInputPort<int>, IColorPort
    {
        public int color;
        public int priority;
        private int inventory;
        
        private IEnergyConduitInteractable tileEntity;
        [JsonIgnore]
        public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public EnergyConduitInputPort(IEnergyConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }

        public void insert(int energy) {
            tileEntity.insertEnergy(energy);
        }

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }
    }

    [System.Serializable]
    public class EnergyConduitOutputPort : IConduitOutputPort<int>, IColorPort
    { 
        public int color;
        public int extractionRate;
        private IEnergyConduitInteractable tileEntity;
        [JsonIgnore]
        public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public EnergyConduitOutputPort(IEnergyConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public int extract() {
            return tileEntity.extractEnergy(extractionRate);
        }

        public int getColor()
        {
            return color;
        }
        
        public void setColor(int color)
        {
            this.color = color;
        }
    }

    public class EnergyConduitPort : ConduitPort<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitPort(EnergyConduitInputPort inPort, EnergyConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
