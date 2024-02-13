using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace ConduitModule {
    [System.Serializable]
    public class ItemConduitOptions : IConduitOptions
    {
        public int inputPriority;
        public ConduitItemFilter inputFilter;
        public int inputColor;
        public int outputPriority;
        public ConduitItemFilter outputFilter;
        public int outputColor;
        public int extractionSpeed;
        public bool roundRobin;
    }

    [System.Serializable]
    public class EnergyConduitOptions : IConduitOptions {
        public int inputPriority;
        public int inputColor;
        public int outputPriority;
        public int outputColor;
    }

    public class SignalConduitOptions : IConduitOptions {
        public int inputColor;
        public int outputColor;
    }
    public class FluidItemConduitOptions : ItemConduitOptions {

    }

    public static class ConduitOptionsFactory {
        public static IConduitOptions deseralizeOption(ConduitItem conduitItem, string data) {
            switch (conduitItem.getType()) {
                case ConduitType.Item:
                    if (data == null) {
                        return new ItemConduitOptions();
                    }
                    try {
                        return JsonConvert.DeserializeObject<ItemConduitOptions>(data);
                    } catch (JsonSerializationException ex) {
                        Debug.LogError(ex);
                        return new ItemConduitOptions();
                    }
                case ConduitType.Fluid:
                    if (data == null) {
                        return new FluidItemConduitOptions();
                    }
                    try {
                        return JsonConvert.DeserializeObject<FluidItemConduitOptions>(data);
                    } catch (JsonSerializationException ex) {
                        Debug.LogError(ex);
                        return new FluidItemConduitOptions();
                    }
                
                case ConduitType.Energy:
                    if (data == null) {
                        return new EnergyConduitOptions();
                    }
                    try {
                        return JsonConvert.DeserializeObject<EnergyConduitOptions>(data);
                    } catch (JsonSerializationException ex) {
                        Debug.LogError(ex);
                        return new EnergyConduitOptions();
                    }
                
                case ConduitType.Signal:
                    if (data == null) {
                        return new SignalConduitOptions();
                    }
                    try {
                        return JsonConvert.DeserializeObject<SignalConduitOptions>(data);
                    } catch (JsonSerializationException ex) {
                        Debug.LogError(ex);
                        return new SignalConduitOptions();
                    }
                default:
                    Debug.LogError("ItemConduitOptionFactory did not handle switch case " + conduitItem.getType().ToString());
                    return null;
            }
        }
    }

}

