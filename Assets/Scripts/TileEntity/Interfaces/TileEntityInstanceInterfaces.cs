using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conduits.Ports;
using RecipeModule;


namespace TileEntity {
    
    public interface ITickableTileEntity : ISoftLoadableTileEntity
    {
        public void TickUpdate();
    }
    
    public interface IPlaceInitializable
    {
        public void PlaceInitialize();
    }

    public interface IClimableTileEntity {
        public int GetSpeed();
    }

    public interface IRightClickableTileEntity
    {
        public void OnRightClick();
    }

    public interface ILeftClickableTileEntity {
        public void OnLeftClick();
        public bool CanInteract();
        public bool CanBreak();
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
        public void OnBreak();
    }
    public interface IMultiBlockTileEntity {
        public void AssembleMultiBlock();
    }

    public interface IStaticTileEntity {
        
    }

    public interface ISoftLoadableTileEntity {

    }

    

}