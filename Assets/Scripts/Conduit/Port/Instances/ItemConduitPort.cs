using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    

    public class ItemConduitInputPort<Interactable,Filter> : IConduitInputPort<ItemSlot>, IColorPort, IPriorityPort where Interactable : IItemConduitInteractable where Filter : IFilter
    {
        private bool enabled;
        public Filter filter;
        public int color;
        public int priority;
        private int inventory;
        private Interactable tileEntity;
        [JsonIgnore]
        public Interactable TileEntity { get => tileEntity; set => tileEntity = value; }
        public bool Enabled { get => enabled; set => enabled = value; }

        public ItemConduitInputPort(Interactable tileEntity) {
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

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }

        public int getPriority()
        {
            return priority;
        }

        public void setPriority(int val)
        {
            priority = val;
        }
    }

    [System.Serializable]
    public class ItemConduitOutputPort<Interactable,Filter> : IConduitOutputPort<ItemSlot>, IColorPort where Interactable : IItemConduitInteractable where Filter : IFilter
    { 
        private bool enabled;
        public int color;
        public int extractAmount = 4;
        public bool roundRobin;
        private int roundRobinIndex;
        public Filter filter;
        private Interactable tileEntity;
        [JsonIgnore]
        public Interactable TileEntity { get => tileEntity; set => tileEntity = value; }
        public bool Enabled { get => enabled; set => enabled = value; }

        public ItemConduitOutputPort (Interactable tileEntity) {
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

        public int getColor()
        {
            return color;
        }

        public void setColor(int color)
        {
            this.color = color;
        }

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }
    }

    public class AbstractItemConduitPort<Interactable,Filter> : ConduitPort<ItemConduitInputPort<Interactable,Filter>, ItemConduitOutputPort<Interactable,Filter>> 
        where Interactable : IItemConduitInteractable 
        where Filter : IFilter
    {
        public AbstractItemConduitPort(ItemConduitInputPort<Interactable,Filter> inPort, ItemConduitOutputPort<Interactable,Filter> outPort) : base(inPort, outPort)
        {
        }
    }
}
