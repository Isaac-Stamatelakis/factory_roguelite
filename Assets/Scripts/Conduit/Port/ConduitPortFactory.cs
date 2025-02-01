using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;
using Items;

namespace Conduits.Ports {
    public enum PortDataType
    {
        ItemInput,
        ItemOutput,
        Standard,
        Priority
    }
    public static class ConduitPortFactory
    {
        public static readonly int PORT_COLORS = 16;
        public static List<TileEntityPortData> GetEntityPorts(IConduitPortTileEntity conduitPortTileEntity, ConduitType type)
        {
            ConduitPortLayout layout = conduitPortTileEntity.GetConduitPortLayout();
            if (layout == null) return null;
            return GetEntityPorts(layout, type);
        }

        public static List<TileEntityPortData> GetEntityPorts(ConduitPortLayout layout, ConduitType type)
        {
            switch (type) {
                case ConduitType.Item:
                    return layout.itemPorts;
                case ConduitType.Fluid:
                    return layout.fluidPorts;
                case ConduitType.Energy:
                    return layout.energyPorts;
                case ConduitType.Signal:
                    return layout.signalPorts;
                case ConduitType.Matrix:
                    return layout.matrixPorts;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static IConduitPort Deserialize(string data, ConduitType conduitType, ConduitItem conduitItem, ITileEntityInstance tileEntityInstance, Vector2Int conduitPosition) {
            if (data == null) {
                return null;
            }
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntityInstance, conduitType);
            if (ReferenceEquals(interactable, null)) return null;
            Vector2Int position = conduitPosition - tileEntityInstance.getCellPosition();
            try
            {
                switch (conduitType) {
                    case ConduitType.Item:
                    case ConduitType.Fluid:
                        if (interactable is not IItemConduitInteractable itemConduitInteractable || conduitItem is not ResourceConduitItem itemConduitItem)
                        {
                            return null;
                        }
                        var serializedItemObject = JsonConvert.DeserializeObject<SerializedTileEntityPort<ItemConduitInputPortData,ItemConduitOutputPortData>>(data);
                        return new ItemTileEntityPort(itemConduitInteractable, position, serializedItemObject.InputPortData, serializedItemObject.OutputPortData, itemConduitItem);
                    case ConduitType.Energy:
                        if (interactable is not IEnergyConduitInteractable energyConduitInteractable || conduitItem is not ResourceConduitItem energyConduitItem)
                        {
                            return null;
                        }
                        var serializedEnergyObject = JsonConvert.DeserializeObject<SerializedTileEntityPort<PriorityConduitPortData,ConduitPortData>>(data);
                        return new EnergyTileEntityPort(energyConduitInteractable, position, serializedEnergyObject.InputPortData, serializedEnergyObject.OutputPortData, energyConduitItem);
                    case ConduitType.Signal:
                        if (interactable is not ISignalConduitInteractable signalConduitInteractable || conduitItem is not SignalConduitItem signalConduitItem)
                        {
                            return null;
                        }
                        var serializedSignalObject = JsonConvert.DeserializeObject<SerializedTileEntityPort<ConduitPortData,ConduitPortData>>(data);
                        return new SignalTileEntityPort(signalConduitInteractable, position, serializedSignalObject.InputPortData, serializedSignalObject.OutputPortData, signalConduitItem);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning($"Could not deserialize conduit port with data {data}\n{e.Message}");
                return null;
            }
        }

        public static string SerializePortConduit(IConduit conduit, ConduitType type) {
            if (conduit is not IPortConduit portConduit) {
                return null;
            }
            
            string portData = SerializePort(portConduit.GetPort(),type);
            int state = conduit.GetState();
            return JsonConvert.SerializeObject(new PortConduitData(state, portData));
        }
        
        public static string SerializePort(IConduitPort port, ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Item:
                case ConduitType.Fluid:
                    ItemTileEntityPort itemTileEntityPort = port as ItemTileEntityPort;
                    if (itemTileEntityPort == null)
                    {
                        return null;
                    }
                    var portItemDataObject = new SerializedTileEntityPort<ItemConduitInputPortData, ItemConduitOutputPortData>(
                        itemTileEntityPort.GetInputData(),
                        itemTileEntityPort.GetOutputData()
                    );
                    return JsonConvert.SerializeObject(portItemDataObject);
                case ConduitType.Energy:
                    EnergyTileEntityPort energyTileEntityPort = port as EnergyTileEntityPort;
                    if (energyTileEntityPort == null)
                    {
                        return null;
                    }
                    var portEnergyDataObject = new SerializedTileEntityPort<PriorityConduitPortData, ConduitPortData>(
                        energyTileEntityPort.GetInputData(),
                        energyTileEntityPort.GetOutputData()
                    );
                    return JsonConvert.SerializeObject(portEnergyDataObject);
                case ConduitType.Signal:
                    SignalTileEntityPort signalTileEntityPort = port as SignalTileEntityPort;
                    if (signalTileEntityPort == null)
                    {
                        return null;
                    }
                    var portSignalDataObject = new SerializedTileEntityPort<ConduitPortData, ConduitPortData>(
                        signalTileEntityPort.GetInputData(),
                        signalTileEntityPort.GetOutputData())
                    ;
                    return JsonConvert.SerializeObject(portSignalDataObject);
                case ConduitType.Matrix:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
        }

        public static ConduitPortData GetDefaultConduitPortData(PortDataType portDataType)
        {
            return portDataType switch
            {
                PortDataType.ItemInput => new ItemConduitInputPortData(0, true, 0, null),
                PortDataType.ItemOutput => new ItemConduitOutputPortData(0, false, 0, null, false, 0,0),
                PortDataType.Standard => new ConduitPortData(0, true),
                PortDataType.Priority => new PriorityConduitPortData(0, true, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(portDataType), portDataType, null)
            };
        }

        public static ConduitPortData GetDefaultConduitPortData(PortDataType portDataType, PortConnectionType portConnectionType, EntityPortType entityPortType)
        {
            switch (portConnectionType)
            {
                case PortConnectionType.Input:
                    if (entityPortType is not EntityPortType.All && entityPortType is not EntityPortType.Input) return null;
                    break;
                case PortConnectionType.Output:
                    if (entityPortType is not EntityPortType.All && entityPortType is not EntityPortType.Output) return null;
                    break;
            }
            return GetDefaultConduitPortData(portDataType);
        }

        public static IConduitPort CreateDefault(ConduitType conduitType, EntityPortType portType, ITileEntityInstance tileEntityInstance, ConduitItem conduitItem, Vector2Int conduitPosition)
        {
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntityInstance, conduitType);
            if (interactable == null) return default;
            
            Vector2Int position = conduitPosition - tileEntityInstance.getCellPosition();
            switch (conduitType) {
                case ConduitType.Item:
                case ConduitType.Fluid:
                    if (interactable is not IItemConduitInteractable itemConduitInteractable || conduitItem is not ResourceConduitItem itemConduitItem) {
                        return null;
                    }

                    ItemConduitInputPortData itemInputPortData =
                        (ItemConduitInputPortData)GetDefaultConduitPortData(PortDataType.ItemInput,
                            PortConnectionType.Input, portType);
                    ItemConduitOutputPortData itemOutputPortData =
                        (ItemConduitOutputPortData)GetDefaultConduitPortData(PortDataType.ItemOutput,
                            PortConnectionType.Output, portType);
                    return new ItemTileEntityPort(itemConduitInteractable, position, itemInputPortData, itemOutputPortData, itemConduitItem);
                case ConduitType.Energy:
                    if (interactable is not IEnergyConduitInteractable energyConduitInteractable || conduitItem is not ResourceConduitItem energyConduitItem) {
                        return null;
                    }

                    PriorityConduitPortData energyInputPortData =
                        (PriorityConduitPortData)GetDefaultConduitPortData(PortDataType.Priority,
                            PortConnectionType.Input, portType); 
                    ConduitPortData energyOutputData = GetDefaultConduitPortData(PortDataType.Standard, PortConnectionType.Output, portType);
                    return new EnergyTileEntityPort(energyConduitInteractable, position, energyInputPortData, energyOutputData, energyConduitItem);
                case ConduitType.Signal:
                    if (interactable is not ISignalConduitInteractable signalConduitInteractable || conduitItem is not SignalConduitItem signalConduitItem) {
                        return null;
                    }

                    ConduitPortData signalInputPortData =
                        (PriorityConduitPortData)GetDefaultConduitPortData(PortDataType.Priority,
                            PortConnectionType.Input, portType);
                    ConduitPortData signalOutputData = GetDefaultConduitPortData(PortDataType.Standard, PortConnectionType.Output, portType);
                    return new SignalTileEntityPort(signalConduitInteractable, position, signalInputPortData, signalOutputData, signalConduitItem);
                default:
                    throw new ArgumentOutOfRangeException(nameof(conduitType), conduitType, null);
            }
        }
        public static Color GetColorFromInt(int index)
        {
            return index switch
            {
                0 => new Color(0f, 1f, 1f, 1f), // Cyan
                1 => new Color(0f, 1f, 0f, 1f), // Green
                2 => new Color(1f, 0f, 0f, 1f), // Red
                3 => new Color(1f, 1f, 0f, 1f), // Yellow
                4 => new Color(1f, 0f, 1f, 1f), // Magenta
                5 => new Color(0f, 0f, 1f, 1f), // Blue
                6 => new Color(1f, 165f / 255f, 0f, 1f), // Orange
                7 => new Color(0f, 1f, 128f / 255f, 1f), // Lime
                8 => new Color(128f / 255f, 0f, 128f / 255f, 1f), // Purple
                9 => new Color(0f, 128f / 255f, 128f / 255f, 1f), // Teal
                10 => new Color(165f / 255f, 42f / 255f, 42f / 255f, 1f), // Brown
                11 => new Color(1f, 192f / 255f, 203f / 255f, 1f), // Pink
                12 => new Color(1f, 1f, 1f, 1f),
                13 => new Color(128f / 255f, 0f, 0f, 1f), // Maroon
                14 => new Color(0f, 0f, 0f, 1f),
                15 => new Color(128f / 255f, 128f / 255f, 128f / 255f, 1f), // Gray
                _ => Color.gray
            };
        }
    }
    
}
