using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ConduitModule.Ports;
using RecipeModule;

namespace TileEntityModule {
    public interface ITickableTileEntity : ISoftLoadable
    {
        public void tickUpdate();
    }

    public interface ILoadedTickableTileEntity {
        public void loadedTickUpdate();
    }

    

    public interface IRightClickableTileEntity
    {
        public void onRightClick();
    }

    public interface ILeftClickableTileEntity {
        public void onLeftClick();
        public bool canInteract();
        public bool canBreak();
    }

    public interface ISoftLoadable {

    }

    public interface ISerializableTileEntity {
        public string serialize();
        public void unserialize(string data);
    }

    public interface ILoadableTileEntity {
        public void load();
        public void unload();
    }

    public interface IBreakActionTileEntity {
        public void onBreak();
    }
    public interface IProcessorTileEntity {
        public RecipeProcessor getRecipeProcessor();
    }

    public interface IMultiBlockTileEntity {
        public void assembleMultiBlock();
    }

}