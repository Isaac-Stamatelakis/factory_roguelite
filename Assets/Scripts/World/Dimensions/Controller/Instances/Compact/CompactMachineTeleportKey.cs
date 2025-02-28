using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimensions {
    public class CompactMachineTeleportKey : IDimensionTeleportKey
    {
        public List<Vector2Int> Path;
        public bool Locked;
        public CompactMachineTeleportKey(List<Vector2Int> path, bool locked)
        {
            this.Path = path;
            this.Locked = locked;
        }

        
    }

}
