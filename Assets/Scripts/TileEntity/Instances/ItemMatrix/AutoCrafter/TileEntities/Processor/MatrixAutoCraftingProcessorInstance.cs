using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    public class MatrixAutoCraftingProcessorInstance : MatrixAutoCraftingChassisInstance<MatrixAutoCraftingProcessor>
    {
        public MatrixAutoCraftingProcessorInstance(MatrixAutoCraftingChassis tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public override void sync(MatrixAutoCraftingCoreInstance core)
        {
            core.TotalProcessors += TileEntityObject.Cores;
            this.core = core;
        }
    }

}
