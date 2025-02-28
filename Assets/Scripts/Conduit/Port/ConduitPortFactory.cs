using System;
using System.Collections;
using System.Collections.Generic;
using Chunks;
using Chunks.Partitions;
using Conduits.Systems;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;
using Items;
using TileMaps.Layer;
using Tiles;

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
        public const int PORT_COLORS = 16;
        
        /// <summary>
        /// Returns a list of spots conduits can connect to tile entities of each chunk
        /// </summary>
        public static Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetTileEntityPorts(ConduitType conduitType, List<SoftLoadedConduitTileChunk> softLoadedChunks) {
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> tileEntityPortData = new Dictionary<ITileEntityInstance, List<TileEntityPortData>>();
            foreach (SoftLoadedConduitTileChunk unloadedChunk in softLoadedChunks) {
                foreach (IChunkPartition partition in unloadedChunk.Partitions) {
                    if (partition is not IConduitTileChunkPartition) {
                        Debug.LogError("Attempted to load non-conduit partition into conduit system");
                        continue;
                    }
                    Dictionary<ITileEntityInstance, List<TileEntityPortData>> partitionPorts = GetEntityPortsFromPartition(partition,conduitType);
                    foreach (KeyValuePair<ITileEntityInstance, List<TileEntityPortData>> kvp in partitionPorts) {
                        tileEntityPortData[kvp.Key] = kvp.Value;
                    }
                }
            }
            return tileEntityPortData;
            
        }
        
        private static Dictionary<ITileEntityInstance, List<TileEntityPortData>> GetEntityPortsFromPartition(IChunkPartition partition, ConduitType type) {
            Dictionary<ITileEntityInstance, List<TileEntityPortData>> ports = new Dictionary<ITileEntityInstance, List<TileEntityPortData>>();
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    ITileEntityInstance tileEntity = partition.GetTileEntity(position);
                    
                    var entityPorts = ConduitPortFactory.GetEntityPorts(partition, tileEntity, type);
                    if (entityPorts == null) {
                        continue;
                    }
                    ports[tileEntity] = entityPorts;
                }
            }
            return ports;
        }
        public static List<TileEntityPortData> GetEntityPorts(IChunkPartition partition, ITileEntityInstance tileEntityInstance, ConduitType type)
        {
            if (tileEntityInstance is not IConduitPortTileEntity conduitPortTileEntity) return null;
     
            ConduitPortLayout layout = conduitPortTileEntity.GetConduitPortLayout();
            if (!layout) return null;
            List<TileEntityPortData> ports = GetEntityPorts(layout, type);
            return RotateEntityPorts(ports, partition, tileEntityInstance.GetPositionInPartition());
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

        public static List<TileEntityPortData> RotateEntityPorts(List<TileEntityPortData> entityPorts, IChunkPartition partition, Vector2Int positionInPartition)
        {
            BaseTileData baseTileData = partition.GetBaseData(positionInPartition);
            if (baseTileData.rotation == 0)
            {
                return entityPorts;
            }
           
            TileItem tileItem = partition.GetTileItem(positionInPartition, TileMapLayer.Base);
            Vector2Int spriteSize = Global.getSpriteSize(tileItem.getSprite());
            List<TileEntityPortData> tileEntityPortDatas = new List<TileEntityPortData>();
            foreach (TileEntityPortData portData in entityPorts)
            {
                Vector2Int rotatedPosition = GetRotationPortPosition(portData.position, baseTileData.rotation, spriteSize);
                TileEntityPortData rotated = new TileEntityPortData(portData.portType,rotatedPosition);
                tileEntityPortDatas.Add(rotated);
            }
            return tileEntityPortDatas;
        }
        /// <summary>
        /// Rotates port positions
        /// </summary>
        /// <param name="portPosition"></param>
        /// <param name="rotation"></param>
        /// <param name="spriteSize">Sprite size of tile entity in world tile space (16px)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Vector2Int GetRotationPortPosition(Vector2Int portPosition, int rotation, Vector2Int spriteSize)
        {
            if (spriteSize.x % 2 == 0 && rotation is 2 or 3)
            {
                portPosition += Vector2Int.left;
            }

            if (spriteSize.y % 2 == 0 && rotation is 1 or 2)
            {
                portPosition += Vector2Int.down;
            }
            switch (rotation)
            {
                case 0:
                    return portPosition;
                case 1:
                    portPosition *= new Vector2Int(1, -1);
                    SwapVector(ref portPosition);
                    return portPosition;
                case 2:
                    portPosition *= new Vector2Int(-1, -1);
                    return portPosition;
                case 3:
                    portPosition *= new Vector2Int(-1, 1);
                    SwapVector(ref portPosition);
                    return portPosition;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static void SwapVector(ref Vector2Int vector2Int)
        {
            (vector2Int.x, vector2Int.y) = (vector2Int.y, vector2Int.x);   
        }
        public static IConduitPort Deserialize(string data, ConduitType conduitType, ConduitItem conduitItem, ITileEntityInstance tileEntityInstance, Vector2Int conduitPosition) {
            if (data == null) {
                return null;
            }
            IConduitInteractable interactable = ConduitFactory.GetInteractableFromTileEntity(tileEntityInstance, conduitType);
            if (ReferenceEquals(interactable, null)) return null;
            Vector2Int position = conduitPosition - tileEntityInstance.GetCellPosition();
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
            
            Vector2Int position = conduitPosition - tileEntityInstance.GetCellPosition();
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
        
        public static Color GetConduitPortColor(ConduitType conduitType) {
            switch (conduitType) {
                case ConduitType.Item:
                    return Color.green;
                case ConduitType.Fluid:
                    return Color.blue;
                case ConduitType.Energy:
                    return Color.yellow;
                case ConduitType.Signal:
                    return Color.red;
                case ConduitType.Matrix:
                    return Color.magenta;
            }
            throw new System.Exception($"Did not cover case for ConduitType {conduitType}");
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
