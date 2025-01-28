using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using Dimensions;

namespace Chunks.Systems {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
        public void initalize(DimController dimController, IntervalVector coveredArea, int dim, Vector2Int offset) {
            TileMapBundleFactory.LoadTileSystemMaps(transform,tileGridMaps);
            TileMapBundleFactory.LoadTileEntityMaps(transform,tileEntityMaps,DimensionManager.Instance.MiscDimAssets.LitMaterial);
            initalizeObject(dimController,coveredArea,dim,offset);
        }
    }

}
