using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class EnergyConduitInputPort : IConduitInputPort<int>, IColorPort, IPriorityPort
    {
        public bool enabled;
        public int color;
        public int priority;
        private int inventory;
        
        private IEnergyConduitInteractable tileEntity;
        [JsonIgnore]
        public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public EnergyConduitInputPort(IEnergyConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }

        public int insert(int energy) {
            return tileEntity.insertEnergy(energy);
        }

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }

        public int getPriority()
        {
            return priority;
        }

        public void setPriority(int val)
        {
            priority = val;
        }
    }

    [System.Serializable]
    public class EnergyConduitOutputPort : IConduitOutputPort<int>, IColorPort
    { 
        public bool enabled;
        public int color;
        [JsonIgnore] public int extractionRate;
        [JsonIgnore] private IEnergyConduitInteractable tileEntity;
        
        [JsonIgnore] public IEnergyConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public EnergyConduitOutputPort(IEnergyConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public ref int extract() {
            return ref tileEntity.getEnergy();
        }

        public int getColor()
        {
            return color;
        }
        
        public void setColor(int color)
        {
            this.color = color;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }
    }

    public class EnergyConduitPort : ConduitPort<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitPort(EnergyConduitInputPort inPort, EnergyConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
