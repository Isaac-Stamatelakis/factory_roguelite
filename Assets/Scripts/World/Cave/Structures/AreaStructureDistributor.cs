using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Structure Distributor",menuName="Generation/Structure/Distributor")]
    public class AreaStructureDistributor : ScriptableObject, IDistributor
    {
        public List<StructureFrequency> structures;
        public void distribute(SeralizedWorldData worldTileData, int seed, int width, int height) {

        }
    }
    [System.Serializable]
    public class StructureFrequency {
        public Structure generatedStructure;
        [Range(0f,1f)]
        public float frequency;
    }
}

