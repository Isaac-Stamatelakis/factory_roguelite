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
using Conduits.Ports;

namespace TileEntity.Instances {
    
    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Door")]
    
    public class Door : TileEntityObject
    {
        public ConduitPortLayout ConduitPortLayout;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new DoorInstance(this,tilePosition,tileItem,chunk);
        }
    }

}