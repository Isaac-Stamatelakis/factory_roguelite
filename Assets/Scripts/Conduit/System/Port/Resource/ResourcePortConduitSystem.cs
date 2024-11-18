using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;

namespace Conduits.Systems
{
    public abstract class ResourcePortConduitSystem<TInPort, TOutPort> : PortConduitSystem<TInPort,TOutPort>
    
        where TInPort : IColorPort 
        where TOutPort : IColorPort
    {
        protected ResourcePortConduitSystem(string id,IConduitSystemManager manager) : base(id,manager)
        {
        }

        protected bool activeThisTick;
        public override void TickUpdate()
        {
            activeThisTick = false;
            foreach (var colorOutputKVP in ColoredOutputPorts) {
                int color = colorOutputKVP.Key;
                var list = colorOutputKVP.Value;
                if (ColoredInputPorts.ContainsKey(color)) {
                    List<TInPort> priorityOrderInputs = ColoredInputPorts[color];
                    foreach (TOutPort outputPort in list) {
                        IterateTickUpdate(outputPort,priorityOrderInputs,color);
                    }
                }
            }
            SetActive(activeThisTick);
        }

        protected abstract void IterateTickUpdate(TOutPort outputPort,List<TInPort> inputPorts, int color);
    }
    
    
    
}

