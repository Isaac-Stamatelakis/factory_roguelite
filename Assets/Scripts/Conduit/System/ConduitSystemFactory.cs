using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ConduitModule.ConduitSystemModule {
    public static class ConduitSystemFactory {
        public static IConduitSystem create(ConduitType type, string id) {
            switch (type) {
                case ConduitType.Item:
                    return new ItemConduitSystem(id);
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            Debug.LogError("ConduitSystemFactory method 'constructSystem' did not handle switch case for '" + type.ToString() + "'");
            return null;
        }

    }
}