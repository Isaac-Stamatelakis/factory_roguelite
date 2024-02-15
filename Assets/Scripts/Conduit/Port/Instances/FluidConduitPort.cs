using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ConduitModule.Ports {
    public class FluidConduitInputPort : IConduitInputPort<ItemSlot>
    {
        public ItemFilter itemFilter;
        public int color;
        public int priority;
        private int inventory;
        private IConduitInteractable tileEntity;

        public IConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public void insert(ItemSlot itemSlot) {
            if (itemFilter != null) {
                if (!itemFilter.filter(itemSlot)) {
                    return;
                }
            }
            TileEntity.insertItem(itemSlot);
        }

    }

    [System.Serializable]
    public class FluidConduitOutputPort : IConduitOutputPort<ItemSlot>
    { 
        public int color;
        public bool roundRobin;
        private int roundRobinIndex;
        public ItemFilter itemFilter;
        private IConduitInteractable tileEntity;

        public IConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public ItemSlot extract() {
            ItemSlot output = TileEntity.extractItem();
            if (itemFilter != null) {
                if (!itemFilter.filter(output)) {
                    return null;
                }
            }
            return output;
        }
    }

    public class FluidConduitPort : ConduitPort<FluidConduitInputPort,FluidConduitOutputPort> {
        
    }
}
