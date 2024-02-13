using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITileEntityItemConduitPort {

}
public interface ITileEntitySignalConduitPort {

}
public interface ITileEntityEnergyConduitPort {

}
public interface ITileEntityFluidConduitPort {

}
public interface ISolidMachineConduitPort : ITileEntityEnergyConduitPort, ITileEntityItemConduitPort, ITileEntitySignalConduitPort {

}
public interface IFluidMachineConduitPort : ITileEntityEnergyConduitPort, ITileEntityFluidConduitPort, ITileEntitySignalConduitPort {

}
public interface ITotalConduitMachinePort : ITileEntityEnergyConduitPort, ITileEntityFluidConduitPort, ITileEntitySignalConduitPort, ITileEntityItemConduitPort {
    
}
