using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace Conduits.Ports {
    public class SignalConduitInputPort : ConduitTransferPort<ISignalConduitInteractable>, IConduitInputPort<int>, IColorPort, IConduitIOPort
    {
        public bool enabled;
        public int color;
        public int priority;

        public SignalConduitInputPort(ISignalConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public void insert(int signal) {
            tileEntity.insertSignal(signal,relativePosition);
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

        public void setTileEntity(TileEntity tileEntity)
        {
            if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                return;
            } 
            this.tileEntity = signalConduitInteractable;
        }
    }
    public class SignalConduitOutputPort : ConduitTransferPort<ISignalConduitInteractable>, IConduitOutputPort<int>, IColorPort, IConduitIOPort
    { 
        public bool enabled;
        public int color;

        public SignalConduitOutputPort(ISignalConduitInteractable tileEntity) : base(tileEntity)
        {
        }

        public int extract() {
            
            return tileEntity.extractSignal(relativePosition);
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

        public void setTileEntity(TileEntity tileEntity)
        {
            if (tileEntity is not ISignalConduitInteractable signalConduitInteractable) {
                return;
            } 
            this.tileEntity = signalConduitInteractable;
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
