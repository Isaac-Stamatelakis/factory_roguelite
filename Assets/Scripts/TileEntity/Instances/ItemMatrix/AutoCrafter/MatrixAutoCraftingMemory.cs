using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixAutoCraftingMemory : TileEntity, IMatrixCraftTile
    {
        [SerializeField] private int memory;

        public void sync(MatrixAutoCraftCore core)
        {
            core.TotalMemory += memory;
        }
    }
}

