using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule.Caves {
    public abstract class CaveTileGenerator : ScriptableObject, ICaveDistributor
    {
        public abstract void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner);
    }

}
