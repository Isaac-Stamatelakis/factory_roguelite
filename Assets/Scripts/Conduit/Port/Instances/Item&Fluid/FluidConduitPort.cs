using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Ports {
    public class FluidFilter : IFilter
    {
        public bool filter(ItemSlot itemSlot)
        {
            throw new System.NotImplementedException();
        }
    }
    public class FluidConduitPort : ConduitPort<FluidConduitInputPort,FLuidConduitOutputPort>
    {
        public FluidConduitPort(FluidConduitInputPort inPort, FLuidConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }

    public class FluidConduitInputPort : IConduitInputPort<ItemSlot>
    {
        public void insert(ItemSlot val)
        {
            throw new System.NotImplementedException();
        }

        public void removeTileEntity()
        {
            throw new System.NotImplementedException();
        }
    }

    public class FLuidConduitOutputPort : IConduitOutputPort<ItemSlot>
    {
        public ItemSlot extract()
        {
            throw new System.NotImplementedException();
        }

        public void removeTileEntity()
        {
            throw new System.NotImplementedException();
        }
    }

}
