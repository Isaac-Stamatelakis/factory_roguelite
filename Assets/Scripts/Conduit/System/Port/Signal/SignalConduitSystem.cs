using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class SignalConduitSystem : PortConduitSystem<SignalConduitInputPort, SignalConduitOutputPort>
    {
        private bool[] colorActivations;
        public SignalConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
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

        public override void TickUpdate()
        {
            bool active = false;
            for (int color = 0; color < colorActivations.Length; color ++) {
                bool systemActive = false;
                if (coloredOutputPorts.TryGetValue(color, out var outputPorts)) {
                    foreach (SignalConduitOutputPort outputPort in outputPorts) {
                        systemActive = outputPort.extract();
                        if (systemActive) {
                            break;
                        }
                    }
                }
                active = active || systemActive;
                if (!coloredPriorityInputs.ContainsKey(color)) continue;
                
                bool currentActivationStatus = colorActivations[color];
                bool signalChange = currentActivationStatus != systemActive;
                if (!signalChange) continue;
                colorActivations[color] = systemActive;
                var inputPorts = coloredPriorityInputs[color];
                foreach (SignalConduitInputPort inputPort in inputPorts) {
                    inputPort.insert(systemActive);
                }
            }
            SetActive(active);
        }

        protected override void addInputPortPostProcessing(SignalConduitInputPort inputPort)
        {
            // No post processing
        }
    }
}

