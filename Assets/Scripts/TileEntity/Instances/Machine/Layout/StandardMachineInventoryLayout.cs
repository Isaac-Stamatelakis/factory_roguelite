using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Machines {
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout/Standard")]
    public class StandardMachineInventoryLayout : InventoryLayout {
        [SerializeField] public List<Vector2Int> itemInputs;
        [SerializeField] public List<Vector2Int> itemOutputs;
        [SerializeField] public List<Vector2Int> fluidInputs;
        [SerializeField] public List<Vector2Int> fluidOutputs;
    }
}