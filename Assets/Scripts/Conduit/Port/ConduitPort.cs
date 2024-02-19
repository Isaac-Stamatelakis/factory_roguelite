using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public interface IConduitPort {
        public void removeTileEntity();
    }

    public interface IConduitInputPort<T> {
        public void insert(T val);
        public void removeTileEntity();

    }

    public interface IColorPort {
        public int getColor();
        public void setColor(int color);
    }
    public interface IConduitOutputPort<T> {
        public T extract();
        public void removeTileEntity();
    }
    public abstract class ConduitPort<InPort,OutPort> : IConduitPort {
        public ConduitPort(InPort inPort, OutPort outPort) {
            this.inputPort = inPort;
            this.outputPort = outPort;
        }
        public InPort inputPort;
        public OutPort outputPort;
        public void removeTileEntity() {
            if (inputPort != null) {
                ((IConduitInputPort<object>) inputPort).removeTileEntity();
            }
            if (outputPort != null) {
                ((IConduitOutputPort<object>) outputPort).removeTileEntity();
            }
            
        }
    }
  
    
    public interface IConduitInteractable {
        public void set(ConduitType conduitType, List<TileEntityPort> vects);
        public ConduitPortLayout getConduitPortLayout();
        
        
    }

    public interface IItemConduitInteractable : IConduitInteractable {
        public ItemSlot extractItem();
        public ItemSlot insertItem(ItemSlot itemSlot);
    }

    public interface IEnergyConduitInteractable : IConduitInteractable {
        public int extractEnergy();
        public bool sendEnergy();
    }
    public interface ISignalConduitInteractable : IConduitInteractable {
        public int extractSignal();
        public bool sendSignal();
    }
    public interface IFluidConduitInteractable : IConduitInteractable {
        public ItemSlot extractFluid();
        public bool insertFluid(ItemSlot itemSlot);
    }

    /// <summary>
    /// Can't be a dictionary as they cannot be serialized
    /// </summary>
    [System.Serializable]
    public class ConduitPortDataCollection {
        public int test;
        public List<TileEntityPort> itemPorts;
        /*
        public bool test;
        public List<ConduitPortData> itemPorts;
        public List<ConduitPortData> fluidPorts;
        public List<ConduitPortData> signalPorts;
        public List<ConduitPortData> energyPorts;
        */
    }
    [System.Serializable]
    public enum EntityPortType {
        All,
        Input,
        Output,
        None
    }
    [System.Serializable]
    public class TileEntityPort {
        
        public EntityPortType portType;
        public Vector2Int position;
        
        public TileEntityPort(EntityPortType type, Vector2Int position) {
            this.portType = type;
            this.position = position;
        }
    }
}
