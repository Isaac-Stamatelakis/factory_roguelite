using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.ConduitSystemModule {
    public static class ConduitSystemFactory {
        public static IConduitSystem create(IConduit conduit) {
            ConduitItem conduitItem = conduit.getConduitItem();
            ConduitType type = conduitItem.getType();
            IConduitSystem system = null;
            switch (type) {
                case ConduitType.Item:
                    system = new ItemConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Fluid:
                    system = new ItemConduitSystem(conduitItem.id);
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }

            if (system == null) {
                Debug.LogError("ConduitSystemFactory method 'constructSystem' did not handle switch case for '" + type.ToString() + "'");
            }
            return system;
        }

    }
}