using TileMaps.Type;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles.StateTiles.Instances {
    
    [CreateAssetMenu(fileName ="T~New Simon Says Tile",menuName="Tile/State/TwoState")]
    public class TwoStateTile : TileBase, ITypeSwitchType, IStateTileSingle
    {
        [SerializeField] public Tile activeTile;
        [SerializeField] public Tile inactiveTile;
        public TileBase GetTileAtState(int state)
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

        public int GetStateAmount()
        {
            return 2;
        }
    }
}

