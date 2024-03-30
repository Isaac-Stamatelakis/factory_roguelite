using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace ConduitModule.Ports {
    public interface IItemConduitInputPort : IColorPort, IPriorityPort {
        public void insert(ItemSlot itemSlot);
        public IItemConduitInteractable getTileEntity();
    }
    public interface IItemConduitOutputPort : IColorPort{
        public ItemSlot extract();
        public int getExtractAmount();
        public IItemConduitInteractable getTileEntity();
    }


    public abstract class ConduitTransferPort<Interactable> where Interactable : IConduitInteractable {
        protected Interactable tileEntity;
        protected Vector2Int relativePosition;
        [JsonIgnore] public Vector2Int RelativePosition {get => relativePosition; set => relativePosition = value;}
        [JsonIgnore] public Interactable TileEntity { get => tileEntity; set => tileEntity = value; }
        public ConduitTransferPort(Interactable tileEntity) {
            this.tileEntity = tileEntity;
        }
    }

    public abstract class ItemConduitInputPort<Interactable,Filter> : 
    ConduitTransferPort<Interactable>, IConduitInputPort<ItemSlot>, IColorPort, IPriorityPort, IItemConduitInputPort, IConduitIOPort 
    
    where Interactable : IItemConduitInteractable where Filter : IFilter
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
            doInsertion(itemSlot);
            
        }

        protected abstract void doInsertion(ItemSlot itemSlot);

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

        public IItemConduitInteractable getTileEntity()
        {
            return tileEntity;
        }

        public void setTileEntity(TileEntity tileEntity)
        {
            if (tileEntity is not Interactable interactable) {
                return;
            }
            this.tileEntity = interactable;
        }
    }

    public class SolidItemConduitInputPort : ItemConduitInputPort<ISolidItemConduitInteractable, ItemFilter>
    {
        public SolidItemConduitInputPort(ISolidItemConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        protected override void doInsertion(ItemSlot itemSlot)
        {
            tileEntity.insertSolidItem(itemSlot,relativePosition);
        }
    }

    public class FluidItemConduitInputPort : ItemConduitInputPort<IFluidConduitInteractable, FluidFilter>
    {
        public FluidItemConduitInputPort(IFluidConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        protected override void doInsertion(ItemSlot itemSlot)
        {
            tileEntity.insertFluidItem(itemSlot,relativePosition);
        }
    }

    [System.Serializable]
    public class ItemConduitOutputPort<Interactable,Filter> : 
    ConduitTransferPort<Interactable>, IConduitOutputPort<ItemSlot>, IColorPort, IItemConduitOutputPort, IConduitIOPort 
    where Interactable : IItemConduitInteractable where Filter : IFilter
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
            ItemSlot output = doExtraction();
            if (filter != null) {
                if (!filter.filter(output)) {
                    return null;
                }
            }
            return output;
        }

        public virtual ItemSlot doExtraction() {
            return null;
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

        public int getExtractAmount()
        {
            return extractAmount;
        }

        public IItemConduitInteractable getTileEntity()
        {
            return tileEntity;
        }

        public void setTileEntity(TileEntity tileEntity)
        {
            if (tileEntity is not Interactable interactable) {
                return;
            }
            this.tileEntity = interactable;
        }
    }

    public class SolidItemConduitOutputPort : ItemConduitOutputPort<ISolidItemConduitInteractable, ItemFilter>
    {
        public SolidItemConduitOutputPort(ISolidItemConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public override ItemSlot doExtraction()
        {
            return tileEntity.extractSolidItem(relativePosition);
        }
    }
    public class FluidItemConduitOutputPort : ItemConduitOutputPort<IFluidConduitInteractable, FluidFilter>
    {
        public FluidItemConduitOutputPort(IFluidConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public override ItemSlot doExtraction()
        {
            return tileEntity.extractFluidItem(relativePosition);
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

    public class SolidItemConduitPort : ConduitPort<SolidItemConduitInputPort, SolidItemConduitOutputPort>
    {
        public SolidItemConduitPort(SolidItemConduitInputPort inPort, SolidItemConduitOutputPort outPort) : base(inPort, outPort)
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

   public class FluidItemConduitPort: ConduitPort<FluidItemConduitInputPort, FluidItemConduitOutputPort> 
    {
        public FluidItemConduitPort(FluidItemConduitInputPort inPort, FluidItemConduitOutputPort outPort) : base(inPort, outPort)
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
