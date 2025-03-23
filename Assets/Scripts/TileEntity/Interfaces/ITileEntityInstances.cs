using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity {
    
    public interface IUITileEntity {
        public AssetReference GetUIAssetReference();
    }

    public interface IAssetTileEntity
    {
        public AssetReference GetAssetReference();
    }

    public interface ITileEntityUI {
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance);
    }
}
