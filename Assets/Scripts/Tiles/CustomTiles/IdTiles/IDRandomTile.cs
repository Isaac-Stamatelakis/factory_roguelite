using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName ="New Tile",menuName="Tile/Random")]
public class IDRandomTile : RandomTile, IIDTile
{
    public string id;
    public string getId()
    {
        return id;
    }

    public void setID(string id)
    {
        this.id = id;
    }

    
}
