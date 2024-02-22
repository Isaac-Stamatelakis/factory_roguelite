using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

    namespace WorldModule.Generation {
    public static class CaveGenerator
    {
        public static void generateCave() {
            if (WorldCreation.dimExists(Global.WorldName,-1)) {
                string path = WorldCreation.getDimPath(Global.WorldName,-1);
                Directory.Delete(path, true);
            }
            WorldCreation.createDimFolder(Global.WorldName,-1);
            WorldTileData worldTileData = Global.CurrentCave.generate(UnityEngine.Random.Range(-2147483648,2147483647));
            WorldGenerationFactory.saveToJson(worldTileData,Global.CurrentCave.getChunkCaveSize(),Global.CurrentCave.getChunkCoveredArea(),-1);
        }
    }

}
