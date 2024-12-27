using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using RecipeModule;


namespace TileEntity {
    public interface ITickableTileEntity
    {
        public void tickUpdate();
    }

    public interface ILoadedTickableTileEntity {
        public void loadedTickUpdate();
    }

    public interface IClimableTileEntity {
        public int getSpeed();
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
    public interface IMultiBlockTileEntity {
        public void assembleMultiBlock();
    }

    public interface IStaticTileEntity {
        
    }

    public interface ISoftLoadableTileEntity {

    }

    

}