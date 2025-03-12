using UnityEngine;

namespace Tiles.Options.Colors
{
    
    public abstract class TileColorOptionObject : ScriptableObject
    {
        public abstract Color GetColor();
    }
}
