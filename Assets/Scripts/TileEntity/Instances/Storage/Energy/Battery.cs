using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.Storage {
    [CreateAssetMenu(fileName = "E~New Battery", menuName = "Tile Entity/Storage/Battery")]
    public class Battery : TileEntityObject
    {
        public ulong Storage;
        public ConduitPortLayout ConduitPortLayout;

        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new BatteryInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

