using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    public class MatrixAutoCraftingMemoryInstance : MatrixAutoCraftingChassisInstance<MatrixAutoCraftingMemory>
    {
        public MatrixAutoCraftingMemoryInstance(MatrixAutoCraftingMemory tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public override void sync(MatrixAutoCraftingCoreInstance core)
        {
            core.TotalMemory += TileEntityObject.Memory;
            this.core = core;
        }
    }
}

