using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.Options.Overlay
{
    [CreateAssetMenu(fileName ="New Tile Wrapper",menuName="Tile/Wrapper")]
    public class TileWrapperObject : ScriptableObject
    {
        public TileBase TileBase;
    }
}
