using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Indicators.Break
{
    [CreateAssetMenu(fileName ="New Break State Tile",menuName="Tile/State/Break")]
    public class BreakIndicatorStateTileSingle : TileBase, IStateTileSingle
    {
        public Tile[] Tiles;

        public TileBase GetTileAtBreakPercent(float percent)
        {
            if (percent < 0.05f) return null;
            int index = Mathf.RoundToInt(percent * (Tiles.Length-1));
            return Tiles[index];
        }
        public TileBase GetTileAtState(int state)
        {
            if (state < 0 || state >= Tiles.Length) return null;
            return Tiles[state];
        }

        public TileBase GetDefaultTile()
        {
            return Tiles[0];
        }

        public int GetStateAmount()
        {
            return Tiles.Length;
        }
    }
}
