using UnityEngine;

namespace TileEntity.Instances.Machine
{
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout")]
    public class MachineLayoutObject : ScriptableObject
    {
        public MachineInventoryOptions SolidInputs;
        public MachineInventoryOptions SolidOutputs;
        public MachineInventoryOptions FluidInputs;
        public MachineInventoryOptions FluidOutputs;
    }

    [System.Serializable]
    public class MachineInventoryOptions
    {
        public bool DefaultOffset = true;
        public Vector2Int Offset;
        public Vector2Int Size;

        public int GetIntSize()
        {
            return Size.x * Size.y;
        }
    }
}