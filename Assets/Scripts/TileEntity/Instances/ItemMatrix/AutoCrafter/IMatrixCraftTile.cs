using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances.Matrix {
    public interface IMatrixCraftTile : ITileUpdateReciever, ILoadableTileEntity
    {
        public void sync(MatrixAutoCraftCore core);
        public void deactivate();
    }
}

