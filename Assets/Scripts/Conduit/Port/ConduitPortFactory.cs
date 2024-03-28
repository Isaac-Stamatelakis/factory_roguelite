using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace ConduitModule.Ports {
    public static class ConduitPortFactory
    {
        public static IConduitPort deseralize(string data, ConduitType conduitType, TileEntity tileEntity, ConduitItem conduitItem) {
            if (data == null) {
                return null;
            }
            switch (conduitType) {
                case ConduitType.Item:
                    SolidItemConduitPort itemConduitPort = JsonConvert.DeserializeObject<SolidItemConduitPort>(data);
                    
                    if (tileEntity is not ISolidItemConduitInteractable itemConduitInteractable) {
                        return null;
                    }
                    if (itemConduitPort == null) {
                        return null;
                    }
                    if (itemConduitPort.inputPort != null) {
                        itemConduitPort.inputPort.TileEntity = itemConduitInteractable;
                    }
                    if (itemConduitPort.outputPort != null) {
                        itemConduitPort.outputPort.TileEntity = itemConduitInteractable;
                    }
                    return itemConduitPort;
                case ConduitType.Fluid:
                    FluidItemConduitPort fluidConduitPort = JsonConvert.DeserializeObject<FluidItemConduitPort>(data);
                    if (tileEntity is not IFluidConduitInteractable fluidConduitInteractable) {
                        return null;
                    }
                    if (fluidConduitPort == null) {
                        return null;
                    }
                    if (fluidConduitPort.inputPort != null) {
                        fluidConduitPort.inputPort.TileEntity = fluidConduitInteractable;
                    }
                    if (fluidConduitPort.outputPort != null) {
                        fluidConduitPort.outputPort.TileEntity = fluidConduitInteractable;
                    }
                    return fluidConduitPort;
                case ConduitType.Energy:
                    EnergyConduitPort energyConduitPort = JsonConvert.DeserializeObject<EnergyConduitPort>(data);
                    if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                        return null;
                    }
                    if (conduitItem is not ResourceConduitItem energyConduit) {
                        return null;
                    }
                    if (energyConduitPort == null) {
                        return null;
                    }
                    if (energyConduitPort.inputPort != null) {
                        energyConduitPort.inputPort.TileEntity = energyConduitInteractable;
                    }
                    if (energyConduitPort.outputPort != null) {
                        energyConduitPort.outputPort.TileEntity = energyConduitInteractable;
                        energyConduitPort.outputPort.extractionRate = energyConduit.maxSpeed;
                    }
                    return energyConduitPort;
                case ConduitType.Signal:
                    SignalConduitPort signalConduitPort = JsonConvert.DeserializeObject<SignalConduitPort>(data);
                    if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                        return null;
                    }
                    if (signalConduitPort == null) {
                        return null;
                    }
                    if (signalConduitPort.inputPort != null) {
                        signalConduitPort.inputPort.TileEntity = signalConduitInteractable;
                    }
                    if (signalConduitPort.outputPort != null) {
                        signalConduitPort.outputPort.TileEntity = signalConduitInteractable;
                    }
                    return signalConduitPort;
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
        }

        public static string serialize(IConduit conduit) {
            if (conduit == null) {
                return null;
            }
            return JsonConvert.SerializeObject(conduit.getPort());
        }

        public static IConduitPort createDefault(ConduitType conduitType, EntityPortType portType, TileEntity tileEntity, ConduitItem conduitItem) {
            switch (conduitType) {
                case ConduitType.Item:
                    if (tileEntity is not ISolidItemConduitInteractable itemConduitInteractable) {
                        return null;
                    }
                    SolidItemConduitInputPort itemInputPort = null;
                    SolidItemConduitOutputPort itemOutputPort = null;
                    if (portType == EntityPortType.All || portType == EntityPortType.Input) {
                        itemInputPort = new SolidItemConduitInputPort(itemConduitInteractable);
                    }
                    if (portType == EntityPortType.All || portType == EntityPortType.Output) {
                        itemOutputPort = new SolidItemConduitOutputPort(itemConduitInteractable);
                    }   
                    SolidItemConduitPort itemConduitPort = new SolidItemConduitPort(itemInputPort,itemOutputPort);
                    return itemConduitPort;
                case ConduitType.Fluid:
                    if (tileEntity is not IFluidConduitInteractable fluidConduitInteractable) {
                        return null;
                    }
                    FluidItemConduitInputPort fluidInputPort = null;
                    FluidItemConduitOutputPort fluidOutputPort = null;
                    if (portType == EntityPortType.All || portType == EntityPortType.Input) {
                        fluidInputPort = new FluidItemConduitInputPort(fluidConduitInteractable);
                    }
                    if (portType == EntityPortType.All || portType == EntityPortType.Output) {
                        fluidOutputPort = new FluidItemConduitOutputPort(fluidConduitInteractable);
                    }   
                    FluidItemConduitPort fluidConduitPort = new FluidItemConduitPort(fluidInputPort,fluidOutputPort);
                    return fluidConduitPort;
                case ConduitType.Energy:
                    if (tileEntity is not IEnergyConduitInteractable energyConduitInteractable) {
                        return null;
                    }
                    EnergyConduitInputPort energyInputPort = null;
                    EnergyConduitOutputPort energyOutputPort = null;
                    if (conduitItem is not ResourceConduitItem energyConduit) {
                        Debug.LogError("Invalid conduit item type for energy conduit");
                        return null;
                    }
                    
                    if (portType == EntityPortType.All || portType == EntityPortType.Input) {
                        energyInputPort = new EnergyConduitInputPort(energyConduitInteractable);
                    }
                    if (portType == EntityPortType.All || portType == EntityPortType.Output) {
                        energyOutputPort = new EnergyConduitOutputPort(energyConduitInteractable);
                        energyOutputPort.extractionRate = energyConduit.maxSpeed;
                    }   
                    EnergyConduitPort energyConduitPort = new EnergyConduitPort(energyInputPort,energyOutputPort);
                    return energyConduitPort;
                case ConduitType.Signal:
                    if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                        return null;
                    }
                    SignalConduitInputPort signalInputPort = null;
                    SignalConduitOutputPort signalOutputPort = null;
                    if (portType == EntityPortType.All || portType == EntityPortType.Input) {
                        signalInputPort = new SignalConduitInputPort(signalConduitInteractable);
                    }
                    if (portType == EntityPortType.All || portType == EntityPortType.Output) {
                        signalOutputPort = new SignalConduitOutputPort(signalConduitInteractable);
                    }   
                    SignalConduitPort signalConduitPort = new SignalConduitPort(signalInputPort,signalOutputPort);
                    return signalConduitPort;
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
            
        }
    }

}
