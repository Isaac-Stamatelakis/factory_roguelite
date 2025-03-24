using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity {
    /// <summary>
    /// TileEntityObjects which implement this interface will have their UIElements automatically loaded and unloaded by the TileEntityAssetRegistry.
    /// Further their instances will instantiate their UIElement when right-clicked.
    /// </summary>
    public interface IUITileEntity {
        public AssetReference GetUIAssetReference();
    }

    /// <summary>
    /// TileEntityObjects which implement this interface will have their Prefabs automatically loaded and unloaded by the TileEntityAssetRegistry.
    /// </summary>
    public interface IAssetTileEntity
    {
        public AssetReference GetAssetReference();
    }

    public interface ITileEntityUI {
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance);
    }
}
