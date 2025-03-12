using UnityEngine;

namespace Tiles.Options.Colors
{
    [CreateAssetMenu(fileName ="I~New RobotObject Item",menuName="Tile/Color/Standard")]
    public class StandardTileColorOptionObject : TileColorOptionObject
    {
        public Color Color;
        public override Color GetColor()
        {
            return Color;
        }
    }
}
