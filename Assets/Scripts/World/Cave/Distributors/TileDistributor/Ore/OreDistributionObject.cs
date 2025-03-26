using System.Collections.Generic;
using Items;
using Items.Transmutable;
using UnityEngine;
using WorldModule.Caves;

namespace World.Cave.TileDistributor.Ore
{
    [CreateAssetMenu(fileName ="New Area Tile Distributor",menuName="Generation/Distribution/Ore")]
    public class OreDistributionObject : ScriptableObject
    {
        public List<OreDistribution> OreDistributions;
    }

    [System.Serializable]
    public class OreDistribution
    {
        public TransmutableItemMaterial Material;
        public TileDistributionData TileDistributionData;
    }

    public class OreTileAggregator : IDistributorTileAggregator
    {
        private TransmutableItemMaterial material;
        public string GetTileId(string currentId)
        {
            string oreId = TransmutableItemUtils.GetOreId(currentId, material);
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            TileItem tileItem = itemRegistry.GetTileItem(oreId);
            return !tileItem ? currentId : oreId;
        }

        public OreTileAggregator(TransmutableItemMaterial material)
        {
            this.material = material;
        }
    }
}