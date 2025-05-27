using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Fluid
{
    [CreateAssetMenu(fileName = "New Fluid Tile", menuName = "Tile/Fluid")]
    public class FluidTile : TileBase, IStateTile
    {
        private const int FLUID_TILE_SIZE = 16;
        [SerializeField] public Tile[] tiles;
        public TileBase GetDefaultTile()
        {
            return tiles[^1];
        }

        public int GetStateAmount()
        {
            return FLUID_TILE_SIZE;
        }
        
        public Tile GetTile(int fill) {
            if (fill == 0) {
                return null;
            }
            fill--;
            return fill switch
            {
                0 => tiles[1],
                < 0 => null,
                _ => fill >= tiles.Length ? tiles[^1] : tiles[fill]
            };
        }
        
        public Tile GetTile(float fill)
        {
            int tileIndex = Mathf.FloorToInt(tiles.Length * fill);
            if (tileIndex == 0) return null;
            tileIndex--;
            return tiles[tileIndex];
        }
        
        
    }
}
