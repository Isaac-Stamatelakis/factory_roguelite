using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class IdAnimatedTile : AnimatedTile, IIDTile
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
