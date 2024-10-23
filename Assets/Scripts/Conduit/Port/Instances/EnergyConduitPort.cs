using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace Conduits.Ports {
    public class EnergyConduitInputPort : ConduitTransferPort<IEnergyConduitInteractable>, IConduitInputPort<int>, IColorPort, IPriorityPort, IConduitIOPort
    {
        public bool enabled;
        public int color;
        public int priority;
        private int inventory;

        public EnergyConduitInputPort(IEnergyConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public int insert(int energy) {
            return tileEntity.insertEnergy(energy,relativePosition);
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

        public void setTileEntity(ITileEntityInstance tileEntity)
        {
            if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                return;
            } 
            this.tileEntity = energyConduitInteractable;
        }
    }

    [System.Serializable]
    public class EnergyConduitOutputPort : ConduitTransferPort<IEnergyConduitInteractable>, IConduitOutputPort<int>, IColorPort, IConduitIOPort
    { 
        public bool enabled;
        public int color;
        [JsonIgnore] public int extractionRate;

        public EnergyConduitOutputPort(IEnergyConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public ref int extract() {
            return ref tileEntity.getEnergy(relativePosition);
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

        public void setTileEntity(ITileEntityInstance tileEntity)
        {
            if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                return;
            }
            this.tileEntity = energyConduitInteractable;
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
