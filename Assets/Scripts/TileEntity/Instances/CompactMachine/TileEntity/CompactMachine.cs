using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;
using UnityEngine.AddressableAssets;
using Chunks.Systems;

namespace TileEntity.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Compact Machine")]
    public class CompactMachine : TileEntityObject, IManagedUITileEntity
    {
        public ConduitPortLayout ConduitPortLayout;
        public string StructurePath;
        public TileEntityUIManager UIManager;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return UIManager;
        }
    }
}

