using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using Dimensions;

namespace Chunks.Systems {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
        public void initalize(DimController dimController, IntervalVector coveredArea, int dim, Vector2Int offset) {
            initalizeObject(dimController,coveredArea,dim,offset);
        }
        public override void Awake()
        {
            base.Awake();
            TileMapBundleFactory.loadTileSystemMaps(transform,tileGridMaps);
        }
        public override void saveOnDestroy()
        {
            base.saveOnDestroy();
        }

        private void initFluidTileMap() {

        }


    }

}
