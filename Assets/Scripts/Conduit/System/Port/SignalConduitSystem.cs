using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class SignalConduitSystem : PortConduitSystem<SignalConduitInputPort, SignalConduitOutputPort>
    {
        private bool[] colorActivations;
        public SignalConduitSystem(string id) : base(id)
        {
            colorActivations = new bool[ConduitPortUIFactory.PORT_COLORS];
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.GetPort();
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

        public override void iterateTickUpdate(SignalConduitOutputPort outputPort, List<SignalConduitInputPort> inputPort, int color)
        {
            
        }

        public override void TickUpdate()
        {
            for (int color = 0; color < colorActivations.Length; color ++) {
                if (!coloredPriorityInputs.ContainsKey(color)) {
                    break;
                }
                bool systemActive = false;
                if (coloredOutputPorts.ContainsKey(color)) {
                    var outputPorts = coloredOutputPorts[color];
                    foreach (SignalConduitOutputPort outputPort in outputPorts) {
                        systemActive = outputPort.extract();
                        if (systemActive) {
                            break;
                        }
                    }
                }
                bool currentActivationStatus = colorActivations[color];
                bool signalChange = currentActivationStatus != systemActive;
                if (signalChange) {
                    colorActivations[color] = systemActive;
                    var inputPorts = coloredPriorityInputs[color];
                    foreach (SignalConduitInputPort inputPort in inputPorts) {
                        inputPort.insert(systemActive);
                    }
                }

            }
        }

        protected override void addInputPortPostProcessing(SignalConduitInputPort inputPort)
        {
            // No post processing
        }
    }
}

