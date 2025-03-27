using System.Collections.Generic;
using Items;
using Tiles;
using UnityEngine;

namespace World.Cave.Distributors.FluidDistributor
{
    [System.Serializable]
    public class FluidAreaDistribution
    {
        public float MinAnchorY;
        public float MaxAnchorY;
        public FluidTileItem Item;
    }

    public class FluidAreaDistributor : ICaveDistributor
    {
        private List<FluidAreaDistribution> areaDistributions;

        public FluidAreaDistributor(List<FluidAreaDistribution> areaDistributions)
        {
            this.areaDistributions = areaDistributions;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            string[,] fluidIds = worldData.fluidData.ids;
            string[,] baseIds = worldData.baseData.ids;
            BaseTileData[,] baseTileData = worldData.baseData.sTileOptions;
            float[,] fills = worldData.fluidData.fill;
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            foreach (FluidAreaDistribution fluidAreaDistribution in areaDistributions)
            {
                string itemId = fluidAreaDistribution.Item?.id;
                if (itemId == null) continue;
                int minY = (int)(height * fluidAreaDistribution.MinAnchorY);
                int maxY = (int)(height * fluidAreaDistribution.MaxAnchorY);
                for (int x = 0; x < width; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        string baseId = baseIds[x, y];
                        if (baseId != null) // Only place fluid tiles in hammer tiles & objects
                        {
                            TileItem tileItem = itemRegistry.GetTileItem(baseId);
                            if (tileItem)
                            {
                                bool hammerTile = tileItem.tile is HammerTile;
                                if (tileItem.tileType == TileType.Block && !hammerTile)
                                {
                                    continue;
                                }
                                if (hammerTile)
                                {
                                    int state = baseTileData[x, y]?.state ?? 0;
                                    if (state == 0) continue;
                                }
                            }
                        }
                        
                        fluidIds[x, y] = itemId;
                        fills[x, y] = 1;
                    }
                }
            }
        }
    }
}
