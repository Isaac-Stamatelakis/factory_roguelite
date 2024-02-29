using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class SignalConduitInputPort : IConduitInputPort<int>, IColorPort
    {
        public int color;
        public int priority;
        
        private ISignalConduitInteractable tileEntity;
        [JsonIgnore]
        public ISignalConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }

        public SignalConduitInputPort(ISignalConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public void insert(int signal) {
            tileEntity.insertSignal(signal);
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
    public class SignalConduitOutputPort : IConduitOutputPort<int>, IColorPort
    { 
        public int color;
        private ISignalConduitInteractable tileEntity;
        [JsonIgnore]
        public ISignalConduitInteractable TileEntity { get => tileEntity; set => tileEntity = value; }
        public SignalConduitOutputPort(ISignalConduitInteractable tileEntity) {
            this.tileEntity = tileEntity;
        }
        public int extract() {
            
            return tileEntity.extractSignal();
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

    public class SignalConduitPort : ConduitPort<SignalConduitInputPort, SignalConduitOutputPort>
    {
        public SignalConduitPort(SignalConduitInputPort inPort, SignalConduitOutputPort outPort) : base(inPort, outPort)
        {
        }
    }
}
