using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Machine {
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Layout")]
    public class MachineLayout : ScriptableObject
    {
        public List<Vector2Int> inputs;
        public List<Vector2Int> outputs;   

    }
}

