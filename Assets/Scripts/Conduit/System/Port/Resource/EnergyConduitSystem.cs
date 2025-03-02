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
            ulong totalEnergy = outputInteractable.GetEnergy(outputPort.Position);
            if (totalEnergy == 0) return;
            activeThisTick = true;
            ulong toInsert;
            ulong extractionRate = outputPort.GetExtractionRate();
            if (totalEnergy >= extractionRate) {
                toInsert = extractionRate;
                totalEnergy -= extractionRate;
            } else {
                toInsert = totalEnergy;
            }
            
            foreach (EnergyTileEntityPort inputPort in inputPorts) {
                if (toInsert == 0)
                {
                    break;
                }
                IEnergyConduitInteractable inputInteractable = inputPort.Interactable;
                ulong taken = inputInteractable.InsertEnergy(toInsert, inputPort.Position);
                toInsert -= taken;
            }
            totalEnergy += toInsert; // Add back left over energy
            outputInteractable.SetEnergy(totalEnergy,outputPort.Position);
        }
    }
}

