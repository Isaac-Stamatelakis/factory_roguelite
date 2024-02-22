using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WorldModule.Generation {
    [CreateAssetMenu(fileName ="New Structure Distributor",menuName="Generation/Structure/Distributor")]
    public class AreaStructureDistributor : ScriptableObject, IDistributor
    {
        public List<StructureFrequency> structures;
        public void distribute(WorldTileData worldTileData, int seed) {

        }
    }
    [System.Serializable]
    public class StructureFrequency {
        public GeneratedStructure generatedStructure;
        [Range(0f,1f)]
        public float frequency;
    }
}

