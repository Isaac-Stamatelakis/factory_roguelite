using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Chunks;
using Dimensions;
using UnityEngine.AddressableAssets;
using Chunks.Systems;
using Items.Tags;

namespace TileEntity.Instances.CompactMachines {
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/Compact Machine")]
    public class CompactMachine : TileEntityObject, IUITileEntity, ITagPlacementTileEntity
    {
        public ConduitPortLayout ConduitPortLayout;
        public string StructurePath;
        public AssetReference AssetReference;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineInstance(this,tilePosition,tileItem,chunk);
        }
        
        public ItemTag GetItemTag()
        {
            return ItemTag.CompactMachine;
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
}

