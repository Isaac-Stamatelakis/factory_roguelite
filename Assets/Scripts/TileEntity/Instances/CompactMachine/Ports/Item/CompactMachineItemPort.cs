using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Item Port", menuName = "Tile Entity/Compact Machine/Port/Item")]
    public class CompactMachineItemPort : TileEntity
    {
        public ConduitPortLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineItemPortInstance(this,tilePosition,tileItem,chunk);
        }
    }

}
