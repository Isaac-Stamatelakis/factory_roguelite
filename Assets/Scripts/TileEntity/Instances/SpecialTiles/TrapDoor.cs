using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "E~New Trap Door", menuName = "Tile Entity/Trap Door")]
    public class TrapDoor : TileEntity, IClickableTileEntity
    {
        public void onClick()
        {
            TileEntityHelper.stateIterate(this,1);
        }
    }
}

