using System.Collections.Generic;
using Items;
using Items.Transmutable;
using Misc.RandomFrequency;
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
        public List<SubOreDistribution> SubDistrubtions;
        public TileDistributionData TileDistributionData;
    }
    
    [System.Serializable]
    public class SubOreDistribution
    {
        public TransmutableItemMaterial Material;
        [Range(0, 1)] public float Fill;
    }

    public class OreTileAggregator : IDistributorTileAggregator
    {
        private TransmutableItemMaterial material;
        private List<SubOreDistribution> subDistrubtions;
        public string GetTileId(string currentId)
        {
            TransmutableItemMaterial randomMaterial = GetRandomMaterial();
            string oreId = TransmutableItemUtils.GetOreId(currentId, randomMaterial);
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            TileItem tileItem = itemRegistry.GetTileItem(oreId);
            return !tileItem ? currentId : oreId;
        }

        private TransmutableItemMaterial GetRandomMaterial()
        {
            if ((subDistrubtions?.Count ?? 0) == 0) return material;
            float ran = Random.Range(0f, 1f);
            foreach (SubOreDistribution subOreDistribution in subDistrubtions)
            {
                if (subOreDistribution.Fill < ran) return subOreDistribution.Material;
                ran -= subOreDistribution.Fill;
            }

            return material;
        }

        public OreTileAggregator(TransmutableItemMaterial material, List<SubOreDistribution> subDistrubtions)
        {
            this.material = material;
            this.subDistrubtions = subDistrubtions;
        }
    }
}