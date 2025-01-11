using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileMaps.Place;
using TileMaps.Layer;
using TileMaps.Type;
using TileEntity.Instances.SimonSays;

namespace Tiles {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/TwoState")]
    public class TwoStateTile : TileBase, IIDTile, ITypeSwitchType, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile activeTile;
        [SerializeField] public Tile inactiveTile;
        
        public Sprite getDefaultSprite()
        {
            return activeTile.sprite;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref UnityEngine.Tilemaps.TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
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
                return inactiveTile;
            } 
            return activeTile;
    
        }
        public TileMapType getStateType(int state)
        {
            return TileMapType.Block;
        }

        public int getStateAmount()
        {
            return 2;
        }
    }
}

