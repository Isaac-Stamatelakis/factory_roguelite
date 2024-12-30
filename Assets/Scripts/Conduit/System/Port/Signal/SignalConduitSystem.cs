using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class SignalConduitSystem : PortConduitSystem<SignalTileEntityPort>
    {
        private bool[] colorActivations;
        public SignalConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
        {
            colorActivations = new bool[ConduitPortFactory.PORT_COLORS];
        }

        public override void TickUpdate()
        {
            bool active = false;
            for (int color = 0; color < colorActivations.Length; color ++) {
                bool systemActive = false;
                if (coloredOutputPorts.TryGetValue(color, out var outputPorts)) {
                    foreach (SignalTileEntityPort outputPort in outputPorts)
                    {
                        ISignalConduitInteractable outputInteractable = outputPort.Interactable;
                        systemActive = outputInteractable.ExtractSignal(outputPort.Position);
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
                foreach (SignalTileEntityPort inputPort in inputPorts) {
                    ISignalConduitInteractable inputInteractable = inputPort.Interactable;
                    inputInteractable.InsertSignal(systemActive, inputPort.Position);
                }
            }
            SetActive(active);
        }

        protected override void AddInputPortPostProcessing(SignalTileEntityPort inputPort)
        {
            // No post processing
        }
    }
}

