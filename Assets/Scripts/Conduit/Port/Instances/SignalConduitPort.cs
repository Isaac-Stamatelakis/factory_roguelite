using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public class SignalConduitInputPort : ConduitTransferPort<ISignalConduitInteractable>, IConduitInputPort<int>, IColorPort
    {
        public bool enabled;
        public int color;
        public int priority;

        public SignalConduitInputPort(ISignalConduitInteractable tileEntity) : base(tileEntity)
        {
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

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }
    }
    public class SignalConduitOutputPort : ConduitTransferPort<ISignalConduitInteractable>, IConduitOutputPort<int>, IColorPort
    { 
        public bool enabled;
        public int color;

        public SignalConduitOutputPort(ISignalConduitInteractable tileEntity) : base(tileEntity)
        {
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

        public bool isEnabled()
        {
            return enabled;
        }

        public void setEnabled(bool val)
        {
            this.enabled = val;
        }
    }

    public class SignalConduitPort : ConduitPort<SignalConduitInputPort, SignalConduitOutputPort>
    {
        public SignalConduitPort(SignalConduitInputPort inPort, SignalConduitOutputPort outPort) : base(inPort, outPort)
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
