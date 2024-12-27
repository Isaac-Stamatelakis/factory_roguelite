using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class EnergyConduitSystem : ResourcePortConduitSystem<EnergyConduitInputPort, EnergyConduitOutputPort>
    {
        public EnergyConduitSystem(string id, IConduitSystemManager manager) : base(id, manager)
        {
        }

        public override void addPort(IPortConduit conduit)
        {
            IConduitPort conduitPort = conduit.GetPort();
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

        protected override void IterateTickUpdate(EnergyConduitOutputPort outputPort, List<EnergyConduitInputPort> inputPorts, int color)
        {
            ref ulong totalEnergy = ref outputPort.Extract();
            ulong toInsert;
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
                ulong taken = inputPort.Insert(toInsert);
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

