using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixCraftTile : ITileItemUpdateReciever, ILoadableTileEntity
    {
        public void sync(MatrixAutoCraftingCoreInstance core);
        public void deactivate();
    }
}

