using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;

namespace ConduitModule.Ports {
    public interface IConduitPort {
        
    }

    public interface IConduitInputPort<T> {
        public void insert(T val);
    }
    public interface IConduitOutputPort<T> {
        public T extract();
    }
    public abstract class ConduitPort<InPort,OutPort> : IConduitPort {
        private IConduitInteractable tileEntity;
        public InPort inputPort;
        public OutPort outputPort;

        public IConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }
    }
  
    
    public interface IConduitInteractable {
        public void set(ConduitType conduitType, List<ConduitPortData> vects);
        public ConduitPortLayout getConduitPortLayout();
        public int extractEnergy();
        public void sendEnergy();
        public ItemSlot extractItem();
        public void insertItem(ItemSlot itemSlot);
    }

    /// <summary>
    /// Can't be a dictionary as they cannot be serialized
    /// </summary>
    [System.Serializable]
    public class ConduitPortDataCollection {
        public int test;
        public List<ConduitPortData> itemPorts;
        /*
        public bool test;
        public List<ConduitPortData> itemPorts;
        public List<ConduitPortData> fluidPorts;
        public List<ConduitPortData> signalPorts;
        public List<ConduitPortData> energyPorts;
        */
    }
    [System.Serializable]
    public enum ConduitPortType {
        All,
        Input,
        Output
    }
    [System.Serializable]
    public class ConduitPortData {
        
        public ConduitPortType portType;
        public Vector2Int position;
        
        public ConduitPortData(ConduitPortType type, Vector2Int position) {
            this.portType = type;
            this.position = position;
        }
    }
}
