using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Hammer Tile",menuName="Tile/State/Hammer")]
    
    public class HammerTile : TileBase, IIDTile, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        
        public Sprite getDefaultSprite()
        {
            return baseTile.sprite;
        }

        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }

        public TileBase getTileAtState(int state)
        {
            if (state == 0) {
                return baseTile;
            } else if (state == 1) {
                return cleanSlab;
            } else if (state == 2) {
                return cleanSlant;
            }
            return null;
        }
        public int getStateAmount()
        {
            return 3;
        }
    }
}

