using TileMaps.Type;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles.StateTiles.Instances {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/TwoState")]
    public class TwoStateTile : TileBase, ITypeSwitchType, IStateTile
    {
        [SerializeField] public string id;
        [SerializeField] public Tile activeTile;
        [SerializeField] public Tile inactiveTile;
        public TileBase getTileAtState(int state)
        {
            if (state == 0) {
                return inactiveTile;
            } 
            return activeTile;
    
        }

        public TileBase GetDefaultTile()
        {
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

