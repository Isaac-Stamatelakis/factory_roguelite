using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;

namespace DevTools.Structures {
    public class BorderStructureOption : StructureGenerationOption
    {
        protected string id;
        public BorderStructureOption(string id) : base()
        {
            this.id = id;
        }

        public override void apply(WorldTileConduitData worldData)
        {
            SerializedBaseTileData baseTileData = worldData.baseData;
            int width = baseTileData.ids.GetLength(0);
            int height = baseTileData.ids.GetLength(1);

            // Set first row
            for (int x = 0; x < width; x++) {
                baseTileData.ids[x, 0] = id; // First row
            }

            // Set last row
            for (int x = 0; x < width; x++) {
                baseTileData.ids[x, height - 1] = id; // Last row
            }

            // Set first column
            for (int y = 0; y < height; y++) {
                baseTileData.ids[0, y] = id; // First column
            }

            // Set last column
            for (int y = 0; y < height; y++) {
                baseTileData.ids[width - 1, y] = id; // Last column
            }
        }
    }
}

