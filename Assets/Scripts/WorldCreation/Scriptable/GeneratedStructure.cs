using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Generation {
    [CreateAssetMenu(fileName ="New Structure",menuName="Generation/Structure/Instance")]
    public class GeneratedStructure : ScriptableObject
    {
        public List<GameObject> tilemapPrefabs;
    }

}
