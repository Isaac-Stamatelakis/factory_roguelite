using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMapModule;
using TileMapModule.Layer;
using Tiles;
using ChunkModule.PartitionModule;
using TileMapModule.Type;

namespace TileEntityModule.Instances {

    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Door")]
    public class Door : TileEntity, IClickableTileEntity
    {
        public void onClick()
        {
            TileEntityHelper.stateIterate(this,2);
        }
    }

}