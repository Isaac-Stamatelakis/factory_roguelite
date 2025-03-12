using System.Collections.Generic;
using UnityEngine;
using WorldModule.Caves;

namespace World.Cave.TileDistributor
{
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Distribution/Tile")]
    public class TileDistributorObject : ScriptableObject
    {
        public List<TileDistribution> TileDistributions;
    }
}
