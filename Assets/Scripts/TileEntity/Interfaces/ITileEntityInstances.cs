using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileEntityModule {
    public interface IManagedUITileEntity {
        public TileEntityUIManager getUIManager();
    }

    public interface IAssetManagerTileEntity {
        public void load();
        public void free();
        public TileEntityAssetManager getAssetManager();
    }

    public interface ITileEntityUI<T> where T : ITileEntityInstance {
        public void display(T tileEntityInstance);
    }
}
