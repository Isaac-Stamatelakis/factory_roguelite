using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

    namespace WorldModule.Caves {
    public static class CaveGenerator
    {
        public static void generateCave() {
            if (WorldLoadUtils.dimExists(-1)) {
                string path = WorldLoadUtils.getDimPath(-1);
                Directory.Delete(path, true);
            }
            WorldLoadUtils.createDimFolder(-1);
            SerializedTileData worldTileData = Global.CurrentCave.generate(UnityEngine.Random.Range(-2147483648,2147483647));
            WorldGenerationFactory.saveToJson(
                worldTileData,
                Global.CurrentCave.getChunkCaveSize(),
                Global.CurrentCave.getChunkCoveredArea(),
                -1,
                WorldLoadUtils.getDimPath(-1)
            );
        }
    }

}
