using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps;
using TileMaps.Layer;
using Tiles;
using Chunks.Partitions;
using TileMaps.Type;
using Chunks;

namespace TileEntityModule.Instances {

    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Door")]
    public class Door : TileEntity
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new DoorInstance(this,tilePosition,tileItem,chunk);
        }
    }

}