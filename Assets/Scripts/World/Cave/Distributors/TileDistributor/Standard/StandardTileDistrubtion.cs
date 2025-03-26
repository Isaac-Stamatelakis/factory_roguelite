using System.Collections.Generic;
using UnityEngine;
using WorldModule.Caves;

namespace World.Cave.TileDistributor.Standard
{
    [System.Serializable]
    public class StandardTileDistrubtion
    {
        public List<TileDistributionFrequency> Tiles;
        public TileDistributionData TileDistributionData;
    }
}
