using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;

namespace Conduits.Systems {
    public class EnergyConduitSystem : ResourceColoredIOPortConduitSystem<EnergyTileEntityPort>
    {
        public EnergyConduitSystem(string id, IConduitSystemManager manager) : base(id, manager)
        {
        }
        
        protected override void AddInputPortPostProcessing(EnergyTileEntityPort inputPort)
        {
            // No post processing
        }

        protected override void IterateTickUpdate(EnergyTileEntityPort outputPort, List<EnergyTileEntityPort> inputPorts, int color)
        {
            IEnergyConduitInteractable outputInteractable = outputPort.Interactable;
            
            ref ulong totalEnergy = ref outputInteractable.GetEnergy(outputPort.Position);
            ulong toInsert;
            ulong extractionRate = outputPort.GetExtractionRate();
            if (totalEnergy >= extractionRate) {
                toInsert = extractionRate;
                totalEnergy -= extractionRate;
            } else {
                toInsert = totalEnergy;
            }

            if (toInsert > 0)
            {
                activeThisTick = true;
            }
            
            foreach (EnergyTileEntityPort inputPort in inputPorts) {
                if (toInsert == 0) {
                    return;
                }
                IEnergyConduitInteractable inputInteractable = inputPort.Interactable;
                ulong taken = inputInteractable.InsertEnergy(toInsert, inputPort.Position);
                toInsert -= taken;
            }
            totalEnergy += toInsert;
        }
    }
}

