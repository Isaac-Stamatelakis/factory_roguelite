using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TileEntityModule {
public interface ITickableTileEntity 
{
    public void tickUpdate();
}

public interface IClickableTileEntity 
{
    public void onClick();
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

}