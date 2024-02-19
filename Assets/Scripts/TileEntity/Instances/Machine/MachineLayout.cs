using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Machine {
    [CreateAssetMenu(fileName = "New Machine", menuName = "Tile Entity/Machine/Layout")]
    public class MachineLayout : ScriptableObject
    {
        public List<Vector2Int> inputs;
        public List<Vector2Int> outputs; 
        public List<InventoryLayout> inventories;
  
    }

    [SerializeField]
    /// <summary>
    /// An Inventory layout is a collection of slots which belong to the same list in a tile entity
    /// </summary>
    public class InventoryLayout {
        public List<Vector2Int> slots;
        public ItemState state;
        [Header("Used for conduits")]
        public int id; 
        public InventoryType inventoryType;
    }
    public enum InventoryType {
        Input,
        Output,
        Other
    }

}

