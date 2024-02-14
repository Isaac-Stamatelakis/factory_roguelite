using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule;

public interface IConduitPort {
    //public void insert(object val);
    //public object extract();
}
public interface IConduitInteractable {
    public void set(ConduitType conduitType, List<ConduitPortData> vects);
    public ConduitPortDataCollection GetConduitPortData();
}

/// <summary>
/// Can't be a dictionary as they cannot be serialized
/// </summary>
[System.Serializable]
public class ConduitPortDataCollection {
    public List<ConduitPortData> itemPorts;
    public List<ConduitPortData> fluidPorts;
    public List<ConduitPortData> signalPorts;
    public List<ConduitPortData> energyPorts;
}
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
public interface IItemConduitPort : IConduitPort {
    public void insert(ItemSlot itemSlot,ItemFilter filter);
    public ItemSlot extract(ItemFilter filter);
}
public interface ISignalConduitPort : IConduitPort {
    public void insert(int signal);
    public int extract();
}
public interface IEnergyConduitPort : IConduitPort {
    public void insert(int energy);
    public int extract();
}
public interface IFluidConduitPort : IConduitPort {
    public void insert(ItemSlot itemSlot,ItemFilter filter);
    public ItemSlot extract(ItemFilter filter);
}
public interface ISolidMachineConduitPort : IEnergyConduitPort, IItemConduitPort, ISignalConduitPort {

}
public interface IFluidMachineConduitPort : IEnergyConduitPort, IFluidConduitPort, ISignalConduitPort {

}
public interface ITotalConduitMachinePort : IEnergyConduitPort, IFluidConduitPort, ISignalConduitPort, IItemConduitPort {
    
}
