using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule;

public interface IConduitPort {
    
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
