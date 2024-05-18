using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities.Mobs;

namespace WorldModule.Caves {
    [System.Serializable]
    public class EntityDistribution 
    {
        public string entityId;
        public int mean;
        public int standardDeviation;
    }
}

