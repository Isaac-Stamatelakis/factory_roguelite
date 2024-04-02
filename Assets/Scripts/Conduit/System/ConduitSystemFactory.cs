using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.Systems {
    public static class ConduitSystemFactory {
        public static IConduitSystem create(IConduit conduit) {
            ConduitItem conduitItem = conduit.getConduitItem();
            ConduitType type = conduitItem.getType();
            IConduitSystem system = null;
            switch (type) {
                case ConduitType.Item:
                    system = new SolidItemConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Fluid:
                    system = new FluidConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Energy:
                    system = new EnergyConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Signal:
                    system = new SignalConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Matrix:
                    system = new MatrixConduitSystem(conduitItem.id);
                    break;
            }

            if (system == null) {
                Debug.LogError("ConduitSystemFactory method 'constructSystem' did not handle switch case for '" + type.ToString() + "'");
            }
            return system;
        }

    }
}