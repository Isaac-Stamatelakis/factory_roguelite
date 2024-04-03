using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Memory", menuName = "Tile Entity/Item Matrix/Crafting/Memory")]
    public class MatrixAutoCraftingMemory : MatrixAutoCraftingChassis
    {
        [SerializeField] private int memory;

        public override void sync(MatrixAutoCraftCore core)
        {
            core.TotalMemory += memory;
            this.core = core;
        }
    }
}

