using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;

namespace ChunkModule.ClosedChunkSystemModule {
    public class TileClosedChunkSystem : ChunkLoadingClosedChunkSystem
    {
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
