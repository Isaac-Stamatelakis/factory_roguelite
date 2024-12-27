using UnityEngine;

namespace TileEntity.Instances.Machine
{
    [CreateAssetMenu(fileName = "New Machine Layout", menuName = "Tile Entity/Machine/Layout")]
    public class MachineLayoutObject : ScriptableObject
    {
        public int SolidInputs;
        public int SolidOutputs;
        public int FluidInputs;
        public int FluidOutputs;
    }
}