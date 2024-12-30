using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;

namespace Conduits.Systems
{
    public interface ITileEntityResourcePort : IColoredTileEntityPort
    {
        public uint GetExtractionRate();
    }
    public abstract class ResourcePortConduitSystem<TTileEntityPort> : PortConduitSystem<TTileEntityPort>
        where TTileEntityPort : IColoredTileEntityPort 
    {
        protected ResourcePortConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
        {
        }

        protected bool activeThisTick;
        public override void TickUpdate()
        {
            activeThisTick = false;
            foreach (var (color, list) in coloredOutputPorts) {
                if (!coloredPriorityInputs.TryGetValue(color, value: out var priorityOrderInputs)) continue;
                foreach (TTileEntityPort outputPort in list) {
                    IterateTickUpdate(outputPort,priorityOrderInputs,color);
                }
            }
            SetActive(activeThisTick);
        }
        
        protected abstract void IterateTickUpdate(TTileEntityPort outputPort,List<TTileEntityPort> inputPorts, int color);
    }
    
    
    
}

