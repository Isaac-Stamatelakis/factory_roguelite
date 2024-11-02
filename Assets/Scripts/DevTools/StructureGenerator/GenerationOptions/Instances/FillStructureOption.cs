using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;

namespace DevTools.Structures {
    public class FillStructureOption : StructureGenerationOption
    {
        protected string id;
        public FillStructureOption(string id) : base()
        {
            this.id = id;
        }

        public override void apply(WorldTileConduitData worldData)
        {
            SerializedBaseTileData baseTileData = worldData.baseData;
            for (int x = 0; x < baseTileData.ids.GetLength(0); x++) {
                for (int y = 0; y < baseTileData.ids.GetLength(1); y++) {
                    baseTileData.ids[x,y] = id;
                }
            }
        }
    }
}

