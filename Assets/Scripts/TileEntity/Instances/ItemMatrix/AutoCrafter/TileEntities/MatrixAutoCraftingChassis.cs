using System.Collections;
using System.Collections.Generic;
using ConduitModule.Ports;
using ConduitModule.Systems;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Crafting Chassis", menuName = "Tile Entity/Item Matrix/Crafting/Chassis")]
    public class MatrixAutoCraftingChassis : TileEntity, IMatrixCraftTile, ISoftLoadable, ILoadableTileEntity
    {
        protected MatrixAutoCraftCore core;
        public virtual void sync(MatrixAutoCraftCore core)
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

