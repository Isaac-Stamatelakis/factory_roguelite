using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;
using Items;

namespace Conduits.Ports {
    public static class ConduitPortFactory
    {
        public static IConduitPort Deserialize(string data, ConduitType conduitType, ConduitItem conduitItem) {
            if (data == null) {
                return null;
            }
            switch (conduitType) {
                case ConduitType.Item:
                    return JsonConvert.DeserializeObject<SolidItemConduitPort>(data);
                case ConduitType.Fluid:
                    return JsonConvert.DeserializeObject<FluidItemConduitPort>(data);
                case ConduitType.Energy:
                    return JsonConvert.DeserializeObject<EnergyConduitPort>(data);
                case ConduitType.Signal:
                    return JsonConvert.DeserializeObject<SignalConduitPort>(data);
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
        }

        public static string Serialize(IConduit conduit) {
            if (conduit is not IPortConduit portConduit) {
                return null;
            }
            string portData = JsonConvert.SerializeObject(portConduit.GetPort());
            int state = conduit.GetState();
            return JsonConvert.SerializeObject(new PortConduitData(state, portData));
        }

        public static IConduitPort CreateDefault(ConduitType conduitType, EntityPortType portType, IConduitInteractable interactable, ConduitItem conduitItem) {
            if (interactable == null)
            {
                return null;
            }
            switch (conduitType) {
                case ConduitType.Item:
                    if (interactable is not ISolidItemConduitInteractable itemConduitInteractable) {
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
                    if (interactable is not IFluidConduitInteractable fluidConduitInteractable) {
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
                    if (interactable is not IEnergyConduitInteractable energyConduitInteractable) {
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
                        energyOutputPort = new EnergyConduitOutputPort(energyConduitInteractable)
                            {
                                extractionRate = (ulong)energyConduit.maxSpeed
                            };
                    }   
                    EnergyConduitPort energyConduitPort = new EnergyConduitPort(energyInputPort,energyOutputPort);
                    return energyConduitPort;
                case ConduitType.Signal:
                    if (interactable is not ISignalConduitInteractable signalConduitInteractable) {
                        return null;
                    }
                    SignalConduitInputPort signalInputPort = null;
                    SignalConduitOutputPort signalOutputPort = null;
                    if (portType is EntityPortType.All or EntityPortType.Input) {
                        signalInputPort = new SignalConduitInputPort(signalConduitInteractable);
                    }
                    if (portType is EntityPortType.All or EntityPortType.Output) {
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
