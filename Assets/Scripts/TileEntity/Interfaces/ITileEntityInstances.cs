using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntity {
    public interface IManagedUITileEntity {
        public TileEntityUIManager getUIManager();
    }

    public interface IAssetManagerTileEntity {
        public void load();
        public void free();
        public TileEntityAssetManager getAssetManager();
    }

    public interface ITileEntityUI<T> where T : ITileEntityInstance {
        public void DisplayTileEntityInstance(T tileEntityInstance);
    }
}
