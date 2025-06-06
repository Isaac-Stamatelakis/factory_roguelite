using System;
using System.Collections;
using System.Collections.Generic;
using Conduits.Systems;
using Item.Slot;
using Items;
using Items.Tags;
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
    
    public abstract class TileEntityConduitPort<TInteractable, TInputData, TOutputData,  TConduitItem> : IColoredTileEntityPort, IOConduitPort
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

        public IConduitInteractable GetInteractable()
        {
            return Interactable;
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

    public interface IFilterConduitPort
    {
        public ItemFilter ItemFilter { get; set; }
    }
    public class ItemConduitInputPortData : PriorityConduitPortData, IFilterConduitPort
    {
        public ItemFilter Filter;
        public ItemConduitInputPortData(int color, bool enabled, int priority, ItemFilter filter) : base(color, enabled, priority)
        {
            Filter = filter;
        }

        public ItemFilter ItemFilter
        {
            get => Filter;
            set => Filter = value;
        }
    }
    
    
    public class ItemConduitOutputPortData : PriorityConduitPortData, IFilterConduitPort
    {
        public ItemFilter Filter;
        public bool RoundRobin;
        public int RoundRobinIndex;
        public uint SpeedUpgrades;

        public ItemConduitOutputPortData(int color, bool enabled, int priority, ItemFilter filter, bool roundRobin, int roundRobinIndex, uint speedUpgrades) : base(color, enabled, priority)
        {
            Filter = filter;
            RoundRobin = roundRobin;
            RoundRobinIndex = roundRobinIndex;
            SpeedUpgrades = speedUpgrades;
        }

        public ItemFilter ItemFilter
        {
            get => Filter;
            set => Filter = value;
        }
    }

    public interface IItemDropConduitPort
    {
        public List<ItemSlot> GetDropItems();
    }
    
    public class ItemTileEntityPort : TileEntityConduitPort<IItemConduitInteractable, ItemConduitInputPortData, ItemConduitOutputPortData, ResourceConduitItem>, ITileEntityResourcePort, IItemDropConduitPort
    {
        public const string FILTER_ID = "item_filter";
        public const string UPGRADE_ID = "conduit_speed_upgrade";
        
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
            return Interactable.ExtractItem(state, Position, outputPortData.Filter);
        }

        public uint GetExtractionRate(ItemState itemState)
        {
            switch (itemState)
            {
                case ItemState.Solid:
                    return (1 + outputPortData.SpeedUpgrades) * Global.SOLID_SPEED_PER_UPGRADE;
                case ItemState.Fluid:
                    return (1 + outputPortData.SpeedUpgrades) * Global.FLUID_SPEED_PER_UPGRADE;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemState), itemState, null);
            }
        }

        public List<ItemSlot> GetDropItems()
        {
            if (outputPortData == null) return new List<ItemSlot>();
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            ItemObject speedUpgrade = itemRegistry.GetItemObject(UPGRADE_ID);
            List<ItemSlot> items = new List<ItemSlot>();
            ItemSlot speedUpgradeSlot = new ItemSlot(speedUpgrade, outputPortData.SpeedUpgrades, null);
            items.Add(speedUpgradeSlot);
            
            if (inputPortData?.Filter != null) AddFilterItem(items, inputPortData?.Filter);
            if (outputPortData?.Filter != null) AddFilterItem(items, outputPortData?.ItemFilter);
            
            return items;

        }

        private void AddFilterItem(List<ItemSlot> items, ItemFilter filter)
        {
            ItemObject filterItem = ItemRegistry.GetInstance().GetItemObject(FILTER_ID);
            ItemSlot inputFilter = new ItemSlot(filterItem, 1, null);
            ItemSlotUtils.AddTag(inputFilter, ItemTag.ItemFilter, filter);
            items.Add(inputFilter);
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
