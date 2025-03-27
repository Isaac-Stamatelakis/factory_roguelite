using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities.Mobs;
using UnityEngine.AddressableAssets;

namespace WorldModule.Caves {
    [System.Serializable]
    public class EntityDistribution 
    {
        public string entityId;
        public int mean;
        public int standardDeviation;
    }
}

