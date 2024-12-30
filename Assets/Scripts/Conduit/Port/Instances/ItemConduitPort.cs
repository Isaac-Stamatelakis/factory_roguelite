using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Systems;
using Items;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;

namespace Conduits.Ports {
    public interface IOConduitPort : IConduitPort
    {
        public ConduitPortData GetPortData(PortConnectionType connectionType);
        public bool HasConnection(PortConnectionType connectionType);
    }
    public interface IItemConduitInteractable : IConduitInteractable
    {
        public ItemSlot ExtractItem(ItemState state, Vector2Int portPosition, ItemFilter filter);
        public void InsertItem(ItemState state, ItemSlot toInsert,Vector2Int portPosition);
    }

    public class SerializedTileEntityPort<TInputData, TOutputData>
        where TInputData : ConduitPortData
        where TOutputData : ConduitPortData
    {
        public TInputData InputPortData;
        public TOutputData OutputPortData;

        public SerializedTileEntityPort(TInputData inputPortData, TOutputData outputPortData)
        {
            InputPortData = inputPortData;
            OutputPortData = outputPortData;
        }
    }
    public abstract class TileEntityConduitPort<TInteractable, TInputData, TOutputData,  TConduitItem> : IColoredTileEntityPort, IConduitPort, IOConduitPort
        where TInteractable : IConduitInteractable
        where TInputData : ConduitPortData
        where TOutputData : ConduitPortData
        where TConduitItem : ConduitItem
    {
        public TInteractable Interactable { get; protected set; }
        public Vector2Int Position;
        public TConduitItem ConduitItem;
        protected TInputData inputPortData;
        protected TOutputData outputPortData;
        protected TileEntityConduitPort(TInteractable interactable, Vector2Int position, TInputData inputPort, TOutputData outputPort, TConduitItem conduitItem)
        {
            this.Interactable = interactable;
            this.Position = position;
            this.inputPortData = inputPort;
            this.outputPortData = outputPort;
            this.ConduitItem = conduitItem;
        }

        public TInputData GetInputData()
        {
            return inputPortData;
        }

        public TOutputData GetOutputData()
        {
            return outputPortData;
        }

        public int GetColor(PortConnectionType portConnectionType)
        {
            return portConnectionType switch
            {
                PortConnectionType.Input => inputPortData.Color,
                PortConnectionType.Output => outputPortData.Color,
                _ => throw new ArgumentOutOfRangeException(nameof(portConnectionType), portConnectionType, null)
            };
        }

        public int SetColor(PortConnectionType portConnectionType, int color)
        {
            return portConnectionType switch
            {
                PortConnectionType.Input => inputPortData.Color = color,
                PortConnectionType.Output => outputPortData.Color = color,
                _ => throw new ArgumentOutOfRangeException(nameof(portConnectionType), portConnectionType, null)
            };
        }

        public ConduitPortData GetPortData(PortConnectionType connectionType)
        {
            return connectionType switch
            {
                PortConnectionType.Input => inputPortData,
                PortConnectionType.Output => outputPortData,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };
        }

        public bool HasConnection(PortConnectionType connectionType)
        {
            return connectionType switch
            {
                PortConnectionType.Input => !ReferenceEquals(inputPortData, null),
                PortConnectionType.Output => !ReferenceEquals(outputPortData, null),
                _ => throw new ArgumentOutOfRangeException(nameof(connectionType), connectionType, null)
            };
        }
    }

    public class ConduitPortData
    {
        public int Color;
        public bool Enabled;
        public ConduitPortData(int color, bool enabled)
        {
            Color = color;
            Enabled = enabled;
        }
    }
    public class PriorityConduitPortData : ConduitPortData
    {
        public int Priority;
        public PriorityConduitPortData(int color, bool enabled, int priority) : base(color, enabled)
        {
            Priority = priority;
        }
    }

    public class ItemConduitInputPortData : PriorityConduitPortData
    {
        public ItemFilter Filter;
        public ItemConduitInputPortData(int color, bool enabled, int priority, ItemFilter filter) : base(color, enabled, priority)
        {
            Filter = filter;
        }
    }
    public class ItemConduitOutputPortData : PriorityConduitPortData
    {
        public ItemFilter Filter;
        public bool RoundRobin;
        public int RoundRobinIndex;

        public ItemConduitOutputPortData(int color, bool enabled, int priority, ItemFilter filter, bool roundRobin, int roundRobinIndex) : base(color, enabled, priority)
        {
            Filter = filter;
            RoundRobin = roundRobin;
            RoundRobinIndex = roundRobinIndex;
        }
    }

    public class ItemTileEntityPort : TileEntityConduitPort<IItemConduitInteractable, ItemConduitInputPortData, ItemConduitOutputPortData, ResourceConduitItem>, ITileEntityResourcePort
    {
        public ItemTileEntityPort(IItemConduitInteractable interactable, Vector2Int position, ItemConduitInputPortData inputPort, ItemConduitOutputPortData outputPort, ResourceConduitItem resourceConduitItem) 
            : base(interactable, position, inputPort, outputPort, resourceConduitItem)
        {
        }
        
        public void Insert(ItemState state, ItemSlot itemSlot) {
            if (ReferenceEquals(inputPortData,null)) return;
            if (!ReferenceEquals(inputPortData.Filter, null) && !inputPortData.Filter.Filter(itemSlot)) return;
            Interactable.InsertItem(state, itemSlot, Position);
            
        }
        
        public ItemSlot Extract(ItemState state) {
            if (ReferenceEquals(outputPortData,null)) return null;
            return Interactable.ExtractItem(state, Position, inputPortData.Filter);
        }

        public uint GetExtractionRate()
        {
            return ConduitItem.maxSpeed;
        }
    }
    
    public class SignalTileEntityPort : TileEntityConduitPort<ISignalConduitInteractable, ConduitPortData, ConduitPortData, SignalConduitItem>
    {
        public SignalTileEntityPort(ISignalConduitInteractable interactable, Vector2Int position, ConduitPortData inputPort, ConduitPortData outputPort, SignalConduitItem signalConduitItem) 
            : base(interactable, position, inputPort, outputPort, signalConduitItem)
        {
        }
    }
}
