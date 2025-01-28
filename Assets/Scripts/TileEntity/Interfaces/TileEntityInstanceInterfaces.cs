using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using RecipeModule;


namespace TileEntity {
    
    public interface ITickableTileEntity : ISoftLoadableTileEntity
    {
        public void tickUpdate();
    }
    
    public interface IPlaceInitializable
    {
        public void PlaceInitialize();
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
        public string Serialize();
        public void Unserialize(string data);
    }

    public interface ILoadableTileEntity {
        public void Load();
        public void Unload();
    }

    public interface IBreakActionTileEntity {
        public void onBreak();
    }
    public interface IMultiBlockTileEntity {
        public void AssembleMultiBlock();
    }

    public interface IStaticTileEntity {
        
    }

    public interface ISoftLoadableTileEntity {

    }

    

}