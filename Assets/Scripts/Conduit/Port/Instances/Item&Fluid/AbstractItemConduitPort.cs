using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public interface IFilter {
        public bool filter(ItemSlot itemSlot);
    }
    [System.Serializable]
    public class ItemConduitInputPort : IConduitInputPort<ItemSlot>, IColorPort
    {
        private bool enabled;
        public ItemFilter filter;
        public int color;
        public int priority;
        private int inventory;
        private IItemConduitInteractable tileEntity;
        [JsonIgnore]
        public IItemConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }
        public bool Enabled { get => enabled; set => enabled = value; }

        public ItemConduitInputPort(IItemConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }

        public void insert(ItemSlot itemSlot) {
            if (filter != null) {
                if (!filter.filter(itemSlot)) {
                    return;
                }
            }
            TileEntity.insertItem(itemSlot);
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }
    }

    [System.Serializable]
    public class ItemConduitOutputPort : IConduitOutputPort<ItemSlot>, IColorPort
    { 
        private bool enabled;
        public int color;
        public int extractAmount = 4;
        public bool roundRobin;
        private int roundRobinIndex;
        public ItemFilter filter;
        private IItemConduitInteractable tileEntity;
        [JsonIgnore]
        public IItemConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }
        public bool Enabled { get => enabled; set => enabled = value; }

        public ItemConduitOutputPort (IItemConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public ItemSlot extract() {
            ItemSlot output = TileEntity.extractItem();
            if (filter != null) {
                if (!filter.filter(output)) {
                    return null;
                }
            }
            return output;
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }
    }

    public class ItemConduitPort : ConduitPort<ItemConduitInputPort, ItemConduitOutputPort>
    {
        public ItemConduitPort(ItemConduitInputPort inPort, ItemConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
