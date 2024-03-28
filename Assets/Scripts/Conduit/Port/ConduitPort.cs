using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public interface IConduitPort {
        public object getInputPort();
        public object GetOutputPort();
        public void setPosition(Vector2Int position);
    }

    public interface IConduitInputPort<T> : ITogglablePort {
        //public void insert(T val);
    }

    public interface IColorPort {
        public int getColor();
        public void setColor(int color);
    }
    public interface ITogglablePort {
        public bool isEnabled();
        public void setEnabled(bool val);
    }
    public interface IPriorityPort {
        public int getPriority();
        public void setPriority(int val);
    }
    public interface IConduitOutputPort<T> : ITogglablePort {
        //public T extract();
        public int getColor();
    }
    public abstract class ConduitPort<InPort,OutPort> : IConduitPort {
        public ConduitPort(InPort inPort, OutPort outPort) {
            this.inputPort = inPort;
            this.outputPort = outPort;
        }
        public InPort inputPort;
        public OutPort outputPort;

        public object getInputPort()
        {
            return inputPort;
        }

        public object GetOutputPort()
        {
            return outputPort;
        }

        public abstract void setPosition(Vector2Int position);

    }

    public interface IItemConduitInteractable : IConduitInteractable {
        
    }
    public interface ISolidItemConduitInteractable : IItemConduitInteractable {
        public ItemSlot extractSolidItem(Vector2Int portPosition);
        public void insertSolidItem(ItemSlot itemSlot,Vector2Int portPosition);
    }

    public interface IEnergyConduitInteractable : IConduitInteractable {
        public int insertEnergy(int energy, Vector2Int portPosition);
        public ref int getEnergy(Vector2Int portPosition);
    }
    public interface ISignalConduitInteractable : IConduitInteractable {
        public int extractSignal(Vector2Int portPosition);
        public void insertSignal(int signal,Vector2Int portPosition);
    }
    public interface IFluidConduitInteractable : IItemConduitInteractable {
        public ItemSlot extractFluidItem(Vector2Int portPosition);
        public void insertFluidItem(ItemSlot itemSlot,Vector2Int portPosition);
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
