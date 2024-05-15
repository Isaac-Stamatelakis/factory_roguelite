using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class EnergyConduitSystem : PortConduitSystem<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitSystem(string id) : base(id)
        {
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.getPort();
            if (conduitPort == null) {
                return;
            }
            if (conduitPort is not EnergyConduitPort energyConduitPort) {
                Debug.LogError("Energy Conduit System recieved non energy conduit port");
                return;
            }
            object input = energyConduitPort.getInputPort();
            if (input != null) {
                addInputPort((EnergyConduitInputPort) energyConduitPort.getInputPort());
            } 
            object output = energyConduitPort.GetOutputPort();
            if (output != null) {
                addOutputPort((EnergyConduitOutputPort) energyConduitPort.GetOutputPort());
            }
        }

        public override void iterateTickUpdate(EnergyConduitOutputPort outputPort, List<EnergyConduitInputPort> inputPorts)
        {

            ref int totalEnergy = ref outputPort.extract();
            int toInsert;
            if (totalEnergy >= outputPort.extractionRate) {
                toInsert = outputPort.extractionRate;
                totalEnergy -= outputPort.extractionRate;
            } else {
                toInsert = totalEnergy;
            }
            
            foreach (EnergyConduitInputPort inputPort in inputPorts) {
                if (toInsert == 0) {
                    return;
                }
                int taken = inputPort.insert(toInsert);
                toInsert -= taken;
            }
            totalEnergy += toInsert;
        }

        protected override void addInputPortPostProcessing(EnergyConduitInputPort inputPort)
        {
            // No post processing
        }
    }
}

