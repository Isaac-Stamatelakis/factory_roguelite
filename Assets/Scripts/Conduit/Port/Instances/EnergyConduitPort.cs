using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class EnergyConduitInputPort : ConduitTransferPort<IEnergyConduitInteractable>, IConduitInputPort<int>, IColorPort, IPriorityPort
    {
        public bool enabled;
        public int color;
        public int priority;
        private int inventory;

        public EnergyConduitInputPort(IEnergyConduitInteractable tileEntity) : base(tileEntity)
        {
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
    public class EnergyConduitOutputPort : ConduitTransferPort<IEnergyConduitInteractable>, IConduitOutputPort<int>, IColorPort
    { 
        public bool enabled;
        public int color;
        [JsonIgnore] public int extractionRate;

        public EnergyConduitOutputPort(IEnergyConduitInteractable tileEntity) : base(tileEntity)
        {
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

        public override void setPosition(Vector2Int position)
        {
            if (inputPort != null) {
                inputPort.RelativePosition = position;
            }
            if (outputPort != null) {
                outputPort.RelativePosition = position;
            }
            
        }
    }
}
