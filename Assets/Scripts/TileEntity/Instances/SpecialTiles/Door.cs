using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps;
using TileMaps.Layer;
using Tiles;
using Chunks.Partitions;
using TileMaps.Type;

namespace TileEntityModule.Instances {

    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Door")]
    public class Door : TileEntity, IRightClickableTileEntity
    {
        public void onRightClick()
        {
            TileEntityHelper.stateIterate(this,2);
        }
    }

}