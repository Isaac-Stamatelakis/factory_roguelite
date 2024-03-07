using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;

namespace ChunkModule.ClosedChunkSystemModule {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
        public void initalize(Transform dimTransform, IntervalVector coveredArea, int dim) {
            initalizeObject(dimTransform,coveredArea,dim);
        }
        public override void Awake()
        {
            base.Awake();
            List<TileMapType> tileMaps = TileMapBundleFactory.getStandardTileTypes();
            foreach (TileMapType tileMapType in tileMaps) {
                initTileMapContainer(tileMapType);
            }
        }

        
    }

}
