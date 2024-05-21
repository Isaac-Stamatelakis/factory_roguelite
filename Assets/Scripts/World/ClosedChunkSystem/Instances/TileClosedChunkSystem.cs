using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;

namespace Chunks.ClosedChunkSystemModule {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
        public void initalize(Transform dimTransform, IntervalVector coveredArea, int dim, Vector2Int offset) {
            initalizeObject(dimTransform,coveredArea,dim,offset);
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
