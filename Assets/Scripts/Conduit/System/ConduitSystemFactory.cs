using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace Conduits.Systems {
    public static class ConduitSystemFactory {
        public static IConduitSystem Create(IConduit conduit, IConduitSystemManager manager) {
            ConduitItem conduitItem = conduit.GetConduitItem();
            ConduitType type = conduitItem.GetConduitType();
            IConduitSystem system = null;
            switch (type) {
                case ConduitType.Item:
                    system = new SolidItemConduitSystem(conduitItem.id, manager);
                    break;
                case ConduitType.Fluid:
                    system = new FluidConduitSystem(conduitItem.id, manager);
                    break;
                case ConduitType.Energy:
                    system = new EnergyConduitSystem(conduitItem.id, manager);
                    break;
                case ConduitType.Signal:
                    system = new SignalConduitSystem(conduitItem.id, manager);
                    break;
                case ConduitType.Matrix:
                    system = new MatrixConduitSystem(conduitItem.id, manager);
                    break;
            }

            if (system == null) {
                Debug.LogError("ConduitSystemFactory method 'constructSystem' did not handle switch case for '" + type.ToString() + "'");
            }
            return system;
        }

    }
}