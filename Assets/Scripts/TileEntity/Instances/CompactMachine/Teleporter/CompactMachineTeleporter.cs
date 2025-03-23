using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Teleporter")]
    public class CompactMachineTeleporter : TileEntityObject
    {
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineTeleporterInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

