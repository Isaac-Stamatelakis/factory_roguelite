using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    public interface IMatrixCraftTile : ITileItemUpdateReciever, ILoadableTileEntity
    {
        public void sync(MatrixAutoCraftingCoreInstance core);
        public void deactivate();
    }
}

