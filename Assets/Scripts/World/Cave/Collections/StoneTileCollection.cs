using System.Collections.Generic;
using Items;
using UnityEngine;

namespace World.Cave.Collections
{
    /// <summary>
    /// Used for the generation of ore tiles
    /// </summary>
    [CreateAssetMenu(fileName ="New Cave",menuName="Generation/Collection/StoneTiles")]
    public class StoneTileCollection : ScriptableObject
    {
        public List<TileItem> Tiles;
    }
}
