using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public class MatrixAutoCraftingChassisInstance<T> : TileEntityInstance<T>, IMatrixCraftTile, ISoftLoadable, ILoadableTileEntity 
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
            TileEntityHelper.stateSwitch(this,0);
        }

        public void load()
        {
            if (core == null || !core.Assembled) {
                TileEntityHelper.stateSwitch(this,0);
            } else {
                TileEntityHelper.stateSwitch(this,1);
            }
        }

        public void unload()
        {
            
        }
    }
}

