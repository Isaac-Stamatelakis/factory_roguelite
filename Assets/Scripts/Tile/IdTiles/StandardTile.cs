using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// Here for easy access to creating regular tiles
[CreateAssetMenu(fileName ="New Tile",menuName="Tile/Tile")]
public class StandardTile : Tile, IIDTile {
    [Header("Useful for Wave Funciton Collapse")]
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
