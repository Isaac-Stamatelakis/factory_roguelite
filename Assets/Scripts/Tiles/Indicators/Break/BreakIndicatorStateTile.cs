using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Indicators.Break
{
    [CreateAssetMenu(fileName ="New Break State Tile",menuName="Tile/State/Break")]
    public class BreakIndicatorStateTile : TileBase, IStateTile
    {
        public Tile[] Tiles;

        public TileBase GetTileAtBreakPercent(float percent)
        {
            int index = Mathf.RoundToInt(percent * (Tiles.Length-1));
            return Tiles[index];
        }
        public TileBase getTileAtState(int state)
        {
            if (state < 0 || state >= Tiles.Length) return null;
            return Tiles[state];
        }

        public TileBase GetDefaultTile()
        {
            return Tiles[0];
        }

        public int getStateAmount()
        {
            return Tiles.Length;
        }
    }
}
