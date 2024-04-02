using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixAutoCraftingProcessor : TileEntity, IMatrixCraftTile
    {
        public void sync(MatrixAutoCraftCore core)
        {
            core.TotalProcessors += 1;
        }
    }
}

