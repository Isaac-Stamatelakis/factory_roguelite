using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITickableTileEntity 
{
    public void tickUpdate();
}

public interface IClickableTileEntity 
{
    public void onClick();
}

public interface ISerializableTileEntity {
    public Dictionary<string,object> serialize();
    public void unserialize(Dictionary<string,object> dict);
}

public interface ILoadableTileEntity {
    public void load();
    public void unload();
}
