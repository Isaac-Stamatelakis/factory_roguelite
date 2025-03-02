using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;
using Item.Slot;

namespace TileEntity.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Item Port", menuName = "Tile Entity/Compact Machine/Port/Item")]
    public class CompactMachineItemPort : CompactMachinePortObject
    {
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineItemPortInstance(this,tilePosition,tileItem,chunk);
        }
    }

}
