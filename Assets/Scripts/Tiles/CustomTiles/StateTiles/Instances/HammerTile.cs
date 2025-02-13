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
        public const int BASE_TILE_STATE = 0;
        public const int SLAB_TILE_STATE = 1;
        public const int SLANT_TILE_STATE = 2;
        public const int STAIR_TILE_STATE = 3;
        
        [SerializeField] public string id;
        [SerializeField] public TileBase baseTile;
        [SerializeField] public Tile cleanSlab;
        [SerializeField] public Tile cleanSlant;
        [SerializeField] public Tile stairs;
        
        public string getId()
        {
            return id;
        }

        public void setID(string id)
        {
            this.id = id;
        }

        public virtual TileBase getTileAtState(int state)
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

        public TileBase GetDefaultTile()
        {
            return baseTile;
        }

        public virtual int getStateAmount()
        {
            return 4;
        }
    }
}

