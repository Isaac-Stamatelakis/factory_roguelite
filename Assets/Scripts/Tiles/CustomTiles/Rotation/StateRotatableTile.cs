using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles {
    public class StateRotatableTile : Tile, IStateRotationTile
    {
        [SerializeField] private TileBase[] tiles;
        public TileBase[] Tiles {get => tiles; set=> tiles = value;}
        
        public TileBase getTile(int rotation, bool mirror)
        {
            int index = rotation%4;
            if (mirror) {
                //index += 2;
                //index ++;
                /*
                if (index == 1) {
                    index = 0;
                } else if (index == 0) {
                    index = 1;
                } else if (index == 2) {
                    index = 3;
                } else if (index == 3) {
                    index = 2;
                }
                */
            }
            index = index % 4;
            if (tiles.Length < index) {
                Debug.LogError($"State Rotatable Tile {name} does not have tile for rotation: {rotation}, mirror : {mirror}");
                return null;
            }
            return tiles[index];
        }
        
    }
}

