using System.Collections.Generic;
using UnityEngine;
using World.Cave.TileDistributor.Standard;
using WorldModule.Caves;

namespace World.Cave.TileDistributor
{
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Distribution/Tile")]
    public class TileDistributorObject : ScriptableObject
    {
        public List<StandardTileDistrubtion> TileDistributions;
    }
}
