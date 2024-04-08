using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Processor", menuName = "Tile Entity/Item Matrix/Crafting/Processor")]
    public class MatrixAutoCraftingProcessor : MatrixAutoCraftingChassis
    {
        public override void sync(MatrixAutoCraftCore core)
        {
            core.TotalProcessors += 1;
            this.core = core;
        }
    }
}

