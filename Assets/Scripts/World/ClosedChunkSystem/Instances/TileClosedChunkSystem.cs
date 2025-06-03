using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using Dimensions;
using Tiles.TileMap.Interval;

namespace Chunks.Systems {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
        public void Initalize(DimController dimController, IntervalVector coveredArea, int dim) {
            TileMapBundleFactory.LoadTileSystemMaps(transform,tileGridMaps);
            TileMapBundleFactory.LoadTileEntityMaps(transform,tileEntityMaps,DimensionManager.Instance.MiscDimAssets.UnlitMaterial);
            InitializeObject(dimController,coveredArea,dim);
        }
    }

}
