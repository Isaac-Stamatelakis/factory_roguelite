using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;

namespace ConduitModule.ConduitSystemModule {
    public class EnergyConduitSystem : ConduitSystem<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitSystem(string id) : base(id)
        {
        }

        public override void addPort(IConduit conduit)
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
                Debug.Log(taken);
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

