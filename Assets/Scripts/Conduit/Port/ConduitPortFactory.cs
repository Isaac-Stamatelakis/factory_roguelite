using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule.Ports {
    public static class ConduitPortFactory
    {
        public static IConduitPort deseralize(string data, ConduitType conduitType) {
            if (data == null) {
                return null;
            }
            switch (conduitType) {
                case ConduitType.Item:
                    return JsonConvert.DeserializeObject<ItemConduitPort>(data);
                case ConduitType.Fluid:
                    break;
                case ConduitType.Energy:
                    break;
                case ConduitType.Signal:
                    break;
            }
            Debug.LogError("ConduitPortFactory method 'fromData' did not handle switch case '" + conduitType.ToString() + "'");
            return null;
        }
    }

}
