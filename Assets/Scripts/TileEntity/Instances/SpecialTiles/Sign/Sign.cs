using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Chunks;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Signs {
    [CreateAssetMenu(fileName = "New Sign", menuName = "Tile Entity/Sign")]
    public class Sign : TileEntityObject, IUITileEntity, IMaterialOverrideUITileEntity
    {
        public AssetReference AssetReference;
        public Material Material;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SignInstance(this,tilePosition,tileItem,chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }

        public Material GetUIMaterial()
        {
            return Material;
        }
    }
    [System.Serializable]
    public class SignData {
        public string line1;
        public string line2;
        public string line3;
        public SignData(string line1, string line2, string line3) {
            this.line1 = line1;
            this.line2 = line2;
            this.line3 = line3;
        }
    }
}

