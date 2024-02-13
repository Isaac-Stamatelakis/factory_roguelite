using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;

namespace ChunkModule.ClosedChunkSystemModule {
    public class TileClosedChunkSystem : ClosedChunkSystem
    {
        public override void Awake()
        {
            base.Awake();
            initTileMapContainer(TileMapType.Block);
            initTileMapContainer(TileMapType.Background);
            initTileMapContainer(TileMapType.Object);
            initTileMapContainer(TileMapType.Platform);
            initTileMapContainer(TileMapType.SlipperyBlock);
            initTileMapContainer(TileMapType.ColladableObject);
            initTileMapContainer(TileMapType.ClimableObject);
        }

        
    }

}
