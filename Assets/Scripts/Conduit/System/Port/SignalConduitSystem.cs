using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class SignalConduitSystem : PortConduitSystem<SignalConduitInputPort, SignalConduitOutputPort>
    {
        private HashSet<SignalConduitInputPort> visited;
        public SignalConduitSystem(string id) : base(id)
        {
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not SignalConduitPort itemPort) {
                Debug.LogError("Item Conduit System recieved non item conduit port");
                return;
            }
            object input = itemPort.getInputPort();
            if (input != null) {
                addInputPort((SignalConduitInputPort) itemPort.getInputPort());
            } 
            object output = itemPort.GetOutputPort();
            if (output != null) {
                addOutputPort((SignalConduitOutputPort) itemPort.GetOutputPort());
            }
        }

        public override void tickUpdate()
        {
            visited = new HashSet<SignalConduitInputPort>();
            base.tickUpdate();
            // Deliver negative signal to each input port which was not visited
            foreach (List<SignalConduitInputPort> inputPorts in ColoredInputPorts.Values) {
                foreach (SignalConduitInputPort inputPort in inputPorts) {
                    if (!visited.Contains(inputPort)) {
                        inputPort.insert(-1);
                    }
                }
            }
        }

        public override void iterateTickUpdate(SignalConduitOutputPort outputPort, List<SignalConduitInputPort> inputPorts)
        {
            int signal = outputPort.extract();
            if (signal <= 0) {
                return;
            }
  
            foreach (SignalConduitInputPort signalConduitInputPort in inputPorts) {
                if (signalConduitInputPort.TileEntity == null) {
                    continue;
                }
                if (signalConduitInputPort.TileEntity.Equals(outputPort.TileEntity)) {
                    continue;
                }
                visited.Add(signalConduitInputPort);
                signalConduitInputPort.insert(signal);
            }
        }

        protected override void addInputPortPostProcessing(SignalConduitInputPort inputPort)
        {
            // No post processing
        }
    }
}

