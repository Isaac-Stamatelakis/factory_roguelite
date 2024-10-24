using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dimensions {
    public class CompactMachineTeleportKey : IDimensionTeleportKey
    {
        private List<Vector2Int> path;
        public CompactMachineTeleportKey(List<Vector2Int> path)
        {
            this.path = path;
        }

        public List<Vector2Int> Path { get => path; set => path = value; }
    }

}
