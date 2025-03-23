using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Chunks;
using Conduits.Systems;
using Items.Tags;
using Items.Tags.Matrix;
using Items.Inventory;
using Entities;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Matrix {
    [CreateAssetMenu(fileName = "E~New Matrix Drive", menuName = "Tile Entity/Item Matrix/Drive")]
    public class MatrixDrive : TileEntityObject, IAssetTileEntity
    {
        public ConduitPortLayout Layout;
        public int rows;
        public int columns;
        public AssetReference PixelPrefabReference;
        private TileEntityAssetManager assetManager;

        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new MatrixDriveInstance(this,tilePosition,tileItem,chunk);
        }

        public void free()
        {
            
        }

        public TileEntityAssetManager getAssetManager()
        {
            return assetManager;
        }

        public void load()
        {
            Dictionary<string, AssetReference> dict = new Dictionary<string, AssetReference>
            {
                { "Pixels", PixelPrefabReference }
            };
            assetManager = new TileEntityAssetManager(dict);
        }

        public AssetReference GetAssetReference()
        {
            return PixelPrefabReference;
        }
    }
}

