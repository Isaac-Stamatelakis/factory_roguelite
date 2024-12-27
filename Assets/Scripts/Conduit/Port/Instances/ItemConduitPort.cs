using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity;

namespace Conduits.Ports {
    public interface IItemConduitInputPort : IColorPort, IPriorityPort {
        public void Insert(ItemSlot itemSlot);
        public IConduitInteractable GetConduitInteractable();
    }
    public interface IItemConduitOutputPort : IColorPort{
        public ItemSlot Extract();
        public uint GetExtractAmount();
        public IConduitInteractable GetConduitInteractable();
    }

    public interface IItemConduitInteractable : IConduitInteractable
    {
        
    }

    public abstract class ConduitTransferPort<TInteractable> where TInteractable : IConduitInteractable {
        protected TInteractable interactable;
        protected Vector2Int relativePosition;
        [JsonIgnore] public Vector2Int RelativePosition {get => relativePosition; set => relativePosition = value;}
        [JsonIgnore] public TInteractable TileEntity { get => interactable; set => interactable = value; }
        protected ConduitTransferPort(TInteractable interactable) {
            this.interactable = interactable;
        }
    }

    public abstract class ItemConduitInputPort<TInteractable,TFilter> : 
    ConduitTransferPort<TInteractable>, IConduitInputPort<ItemSlot>, IColorPort, IPriorityPort, IItemConduitInputPort, IConduitIOPort 
    
    where TInteractable : IItemConduitInteractable where TFilter : IFilter
    {
        private bool enabled;
        public TFilter filter;
        public int color;
        public int priority;
        private int inventory;

        protected ItemConduitInputPort(TInteractable interactable) : base(interactable)
        {

        }

        public bool Enabled { get => enabled; set => enabled = value; }
        
        public void Insert(ItemSlot itemSlot) {
            if (filter != null) {
                if (!filter.Filter(itemSlot)) {
                    return;
                }
            }
            DoInsertion(itemSlot);
            
        }

        protected abstract void DoInsertion(ItemSlot itemSlot);

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

        public IConduitInteractable GetConduitInteractable()
        {
            return interactable;
        }

        public void setTileEntity(ITileEntityInstance tileEntity)
        {
            if (tileEntity is not TInteractable interactable) {
                return;
            }
            this.interactable = interactable;
        }
    }

    public class SolidItemConduitInputPort : ItemConduitInputPort<ISolidItemConduitInteractable, ItemFilter>
    {
        public SolidItemConduitInputPort(ISolidItemConduitInteractable interactable) : base(interactable)
        {
        }

        protected override void DoInsertion(ItemSlot itemSlot)
        {
            interactable.InsertSolidItem(itemSlot,relativePosition);
        }
    }

    public class FluidItemConduitInputPort : ItemConduitInputPort<IFluidConduitInteractable, FluidFilter>
    {
        public FluidItemConduitInputPort(IFluidConduitInteractable interactable) : base(interactable)
        {
        }

        protected override void DoInsertion(ItemSlot itemSlot)
        {
            interactable.InsertFluidItem(itemSlot,relativePosition);
        }
    }

    [System.Serializable]
    public class ItemConduitOutputPort<TInteractable,TFilter> : 
    ConduitTransferPort<TInteractable>, IConduitOutputPort<ItemSlot>, IColorPort, IItemConduitOutputPort, IConduitIOPort 
    where TInteractable : IItemConduitInteractable where TFilter : IFilter
    { 
        private bool enabled;
        public int color;
        public uint extractAmount = 4;
        public bool roundRobin;
        private int roundRobinIndex;
        public TFilter filter;
        public bool Enabled { get => enabled; set => enabled = value; }
        public ItemConduitOutputPort (TInteractable interactable) : base(interactable) {
            this.interactable = interactable;
        }
        public ItemSlot Extract() {
            ItemSlot output = doExtraction();
            if (filter != null) {
                if (!filter.Filter(output)) {
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

        public uint GetExtractAmount()
        {
            return extractAmount;
        }

        public IConduitInteractable GetConduitInteractable()
        {
            return interactable;
        }

        public void setTileEntity(ITileEntityInstance tileEntity)
        {
            if (tileEntity is not TInteractable interactable) {
                return;
            }
            this.interactable = interactable;
        }
    }

    public class SolidItemConduitOutputPort : ItemConduitOutputPort<ISolidItemConduitInteractable, ItemFilter>
    {
        public SolidItemConduitOutputPort(ISolidItemConduitInteractable interactable) : base(interactable)
        {
        }

        public override ItemSlot doExtraction()
        {
            return interactable.ExtractSolidItem(relativePosition);
        }
    }
    public class FluidItemConduitOutputPort : ItemConduitOutputPort<IFluidConduitInteractable, FluidFilter>
    {
        public FluidItemConduitOutputPort(IFluidConduitInteractable interactable) : base(interactable)
        {
        }

        public override ItemSlot doExtraction()
        {
            return interactable.ExtractFluidItem(relativePosition);
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
