using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    [System.Serializable]
    public class ItemConduitInputPort : IConduitInputPort<ItemSlot>
    {
        public ItemFilter itemFilter;
        public int color;
        public int priority;
        private int inventory;
        private IItemConduitInteractable tileEntity;
        [JsonIgnore]
        public IItemConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }
        public ItemConduitInputPort(IItemConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }

        public void insert(ItemSlot itemSlot) {
            if (itemFilter != null) {
                if (!itemFilter.filter(itemSlot)) {
                    return;
                }
            }
            ItemSlot reciever = TileEntity.insertItem(itemSlot);
            if (reciever == null) {
                return;
            }

        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

    }

    [System.Serializable]
    public class ItemConduitOutputPort : IConduitOutputPort<ItemSlot>
    { 
        public int color;
        public int extractAmount = 4;
        public bool roundRobin;
        private int roundRobinIndex;
        public ItemFilter itemFilter;
        private IItemConduitInteractable tileEntity;
        [JsonIgnore]
        public IItemConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public ItemConduitOutputPort (IItemConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public ItemSlot extract() {
            ItemSlot output = TileEntity.extractItem();
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

    public class ItemConduitPort : ConduitPort<ItemConduitInputPort, ItemConduitOutputPort>
    {
        public ItemConduitPort(ItemConduitInputPort inPort, ItemConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
