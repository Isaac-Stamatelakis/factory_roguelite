using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class FluidTileItem : ItemObject
{
    public override Sprite getSprite()
    {
        if (tiles.Length < 0) {
            return null;
        }
        return tiles[tiles.Length-1].sprite;
    }
    public Tile getTile(int fill) {
        if (fill == 0) {
            return null;
        }
        fill--;
        if(fill >= tiles.Length) {
            return tiles[tiles.Length-1];
        }
        return tiles[fill];
    }
    [SerializeField] public Tile[] tiles;
    [SerializeField] public FluidOptions fluidOptions;
}
[System.Serializable]
public class FluidOptions {
    [SerializeField] private int viscosity;
    [SerializeField] private bool invertedGravity;
    public FluidOptions(int viscosity, bool invertedGravity) {
        this.viscosity = viscosity;
        this.invertedGravity = invertedGravity;
    }

    public int Viscosity { get => viscosity;}
    public bool InvertedGravity { get => invertedGravity;}
}


