using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    public class MatrixAutoCraftingChassisInstance<T> : TileEntityInstance<T>, IMatrixCraftTile, ILoadableTileEntity 
        where T : MatrixAutoCraftingChassis
    {
        protected MatrixAutoCraftingCoreInstance core;

        public MatrixAutoCraftingChassisInstance(MatrixAutoCraftingChassis tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base((T)tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public virtual void sync(MatrixAutoCraftingCoreInstance core)
        {
            this.core = core;
        }
        public void tileUpdate(TileItem tileItem)
        {
            MatrixAutoCraftHelper.tileUpdate(tileItem,core);
        }
        public void deactivate()
        {
            TileEntityUtils.stateSwitch(this,0);
        }

        public void Load()
        {
            if (core == null || !core.Assembled) {
                TileEntityUtils.stateSwitch(this,0);
            } else {
                TileEntityUtils.stateSwitch(this,1);
            }
        }

        public void Unload()
        {
            
        }
    }
}

