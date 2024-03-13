using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace ConduitModule.Ports {

    public abstract class ConduitTransferPort<Interactable> where Interactable : IConduitInteractable {
        protected Interactable tileEntity;
        protected Vector2Int relativePosition;
        [JsonIgnore] public Vector2Int RelativePosition {get => relativePosition; set => relativePosition = value;}
        [JsonIgnore] public Interactable TileEntity { get => tileEntity; set => tileEntity = value; }
        public ConduitTransferPort(Interactable tileEntity) {
            this.tileEntity = tileEntity;
        }
    }

    public class ItemConduitInputPort<Interactable,Filter> : ConduitTransferPort<Interactable>, IConduitInputPort<ItemSlot>, IColorPort, IPriorityPort where Interactable : IItemConduitInteractable where Filter : IFilter
    {
        private bool enabled;
        public Filter filter;
        public int color;
        public int priority;
        private int inventory;

        public ItemConduitInputPort(Interactable tileEntity) : base(tileEntity)
        {
        }

        public bool Enabled { get => enabled; set => enabled = value; }

        

        public void insert(ItemSlot itemSlot) {
            if (filter != null) {
                if (!filter.filter(itemSlot)) {
                    return;
                }
            }
            tileEntity.insertItem(itemSlot,relativePosition);
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
    public class ItemConduitOutputPort<Interactable,Filter> : ConduitTransferPort<Interactable>, IConduitOutputPort<ItemSlot>, IColorPort where Interactable : IItemConduitInteractable where Filter : IFilter
    { 
        private bool enabled;
        public int color;
        public int extractAmount = 4;
        public bool roundRobin;
        private int roundRobinIndex;
        public Filter filter;
        public bool Enabled { get => enabled; set => enabled = value; }
        public ItemConduitOutputPort (Interactable tileEntity) : base(tileEntity) {
            this.tileEntity = tileEntity;
        }
        public ItemSlot extract() {
            ItemSlot output = TileEntity.extractItem(relativePosition);
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

        public override void setPosition(Vector2Int position)
        {
            if (inputPort != null) {
                inputPort.RelativePosition = position;
            }
            if (outputPort != null) {
                outputPort.RelativePosition = position;
            }
        }
        
    }
}
