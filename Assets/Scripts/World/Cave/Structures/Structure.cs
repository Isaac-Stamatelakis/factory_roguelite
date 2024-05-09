using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public class Structure : ScriptableObject
    {
        public List<StructureVariant> structureVariants;
    }

    public class StructureVariant {
        public string seralizedWorldTileData;
        public Vector2Int size;
        public int frequency;
    }

}
