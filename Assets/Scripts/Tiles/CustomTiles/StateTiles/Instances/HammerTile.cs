using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;
using UnityEngine.Serialization;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Hammer Tile",menuName="Tile/State/Hammer")]
    
    public class HammerTile : TileBase, IIDTile, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        [SerializeField] public Tile stairs;
        
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
            return state switch
            {
                0 => baseTile,
                1 => cleanSlab,
                2 => cleanSlant,
                3 => stairs,
                _ => null
            };
        }
        public int getStateAmount()
        {
            return 4;
        }
    }
}

