using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityHelper
{
    public static GameObject getTileEntity(Transform tileEntityContainer, string tileContainerName, Vector2Int position) {
        
        return Global.findChild(tileEntityContainer, tileContainerName + "["+ position.x + "," + position.y + "]");
    }
}
