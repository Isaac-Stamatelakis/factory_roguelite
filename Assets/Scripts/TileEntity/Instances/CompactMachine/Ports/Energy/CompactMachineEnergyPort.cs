using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances.Storage;
using Conduits.Ports;
using Newtonsoft.Json;
using Chunks;

namespace TileEntity.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Energy Port", menuName = "Tile Entity/Compact Machine/Port/Energy")]
    public class CompactMachineEnergyPort : TileEntityObject
    {
        public ConduitPortLayout Layout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineEnergyPortInstance(this,tilePosition,tileItem,chunk);
        }
    }

}
