using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ConduitModule.Ports {
    public class SignalConduitInputPort : IConduitInputPort<int>
    {
        public int color;
        public int priority;
        private int inventory;
        private IConduitInteractable tileEntity;

        public IConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public void insert(int itemSlot) {
            
        }

    }
    public class SignalConduitOutputPort : IConduitOutputPort<int>
    { 
        public int color;
        public bool roundRobin;
        private int roundRobinIndex;
        private IConduitInteractable tileEntity;

        public IConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public int extract() {
            
            return 0;
        }
    }

    public class SignalConduitPort : ConduitPort<SignalConduitInputPort,SignalConduitOutputPort> {
        
    }
}
