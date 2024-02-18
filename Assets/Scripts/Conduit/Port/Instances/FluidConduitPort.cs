using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class FluidConduitInputPort : IConduitInputPort<ItemSlot>
    {
        public ItemFilter itemFilter;
        public int color;
        public int priority;
        private int inventory;
        
        private IFluidConduitInteractable tileEntity;
        [JsonIgnore]
        public IFluidConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public void insert(ItemSlot itemSlot) {
            if (itemFilter != null) {
                if (!itemFilter.filter(itemSlot)) {
                    return;
                }
            }
            TileEntity.insertFluid(itemSlot);
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

    }

    [System.Serializable]
    public class FluidConduitOutputPort : IConduitOutputPort<ItemSlot>
    { 
        public int color;
        public bool roundRobin;
        private int roundRobinIndex;
        public ItemFilter itemFilter;
        private IFluidConduitInteractable tileEntity;
        [JsonIgnore]
        public IFluidConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public ItemSlot extract() {
            ItemSlot output = TileEntity.extractFluid();
            if (itemFilter != null) {
                if (!itemFilter.filter(output)) {
                    return null;
                }
            }
            return output;
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }
    }

    public class FluidConduitPort : ConduitPort<FluidConduitInputPort, FluidConduitOutputPort>
    {
        public FluidConduitPort(FluidConduitInputPort inPort, FluidConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
