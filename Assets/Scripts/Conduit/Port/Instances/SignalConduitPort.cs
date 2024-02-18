using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class SignalConduitInputPort : IConduitInputPort<int>
    {
        public int color;
        public int priority;
        private int inventory;
        
        private ISignalConduitInteractable tileEntity;
        [JsonIgnore]
        public ISignalConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public void insert(int itemSlot) {
            
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }

    }
    public class SignalConduitOutputPort : IConduitOutputPort<int>
    { 
        public int color;
        public bool roundRobin;
        private int roundRobinIndex;
        private ISignalConduitInteractable tileEntity;
        [JsonIgnore]
        public ISignalConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public int extract() {
            
            return 0;
        }
        public void removeTileEntity()
        {
            tileEntity = null;
        }
    }

    public class SignalConduitPort : ConduitPort<SignalConduitInputPort, SignalConduitOutputPort>
    {
        public SignalConduitPort(SignalConduitInputPort inPort, SignalConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
