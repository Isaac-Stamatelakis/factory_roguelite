using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.CustomTiles
{
    /// <summary>
    /// Checks tiles next 
    /// </summary>
    public class BackgroundRuleTile : RuleTile
    {
        public override bool RuleMatch(int neighbor, TileBase other)
        {
            switch (neighbor)
            {
                case TilingRuleOutput.Neighbor.This: return other != null;
                case TilingRuleOutput.Neighbor.NotThis: return other == null;
            }

            return true;
        }
    }
}
