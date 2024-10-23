using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Teleporter")]
    public class CompactMachineTeleporter : TileEntity
    {
        public TileEntityUIManager UIManager;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineTeleporterInstance(this,tilePosition,tileItem,chunk);
        }
    }
}

