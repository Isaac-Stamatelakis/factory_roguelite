using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity.Instances.Matrix {
    public static class MatrixAutoCraftHelper
    {
        public static void tileUpdate(TileItem tileItem, MatrixAutoCraftingCoreInstance core) {
            
            if (core == null) {
                return;
            }
            core.AssembleMultiBlock();
            
        }
    }

}
