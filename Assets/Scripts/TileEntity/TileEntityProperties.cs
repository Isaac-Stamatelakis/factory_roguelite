using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
A tileEntity is a tileObject with special properties
**/
public class TileEntityProperties : MonoBehaviour
{
    private Dictionary<string, object> data;
    public Dictionary<string,object> Data {get{return data;} set{data=value;}}
    private string tileContainerName;
    public string TileContainerName {get{return tileContainerName;} set{tileContainerName = value;}}
    private Vector2Int position;
    public Vector2Int Position {get{return position;} set{position=value;}}

    
}
