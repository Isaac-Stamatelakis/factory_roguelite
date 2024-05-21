using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public abstract class CaveTileGenerator : ScriptableObject, ICaveDistributor
    {
        public abstract void distribute(SeralizedWorldData seralizedWorldData, int width, int height, Vector2Int bottomLeftCorner);
    }

}
