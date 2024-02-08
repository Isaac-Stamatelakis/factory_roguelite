using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;

public static class ProcGenHelper {
    public static void saveToJson(WorldTileData worldTileData, Cave cave) {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        int tileMaxX = Global.ChunkSize*caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;
        int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
        int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
        for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
            for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                List<ChunkPartitionData> chunkPartitionDataList = new List<ChunkPartitionData>();
                for (int partitionX = 0; partitionX < Global.PartitionsPerChunk; partitionX ++) {
                    for (int partitionY = 0; partitionY < Global.PartitionsPerChunk; partitionY ++) {
                        int xStart = partitionX*Global.ChunkPartitionSize + Global.ChunkSize * (chunkX-minX);
                        int yStart = partitionY*Global.ChunkPartitionSize + Global.ChunkSize * (chunkY-minY);
                        SerializedTileData partitionData = new SerializedTileData();
                        // TODO ENTITIES
                        partitionData.entityData = new List<EntityData>(); 
                        
                        partitionData.baseData = new SeralizedChunkTileData();
                        partitionData.baseData.ids = new List<List<string>>();
                        partitionData.baseData.sTileOptions = new List<List<Dictionary<string, object>>>();
                        partitionData.baseData.sTileEntityOptions = new List<List<string>>();

                        partitionData.backgroundData = new SeralizedChunkTileData();
                        partitionData.backgroundData.ids = new List<List<string>>();
                        partitionData.backgroundData.sTileOptions = new List<List<Dictionary<string, object>>>();
                        partitionData.backgroundData.sTileEntityOptions = new List<List<string>>();

                        for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
                            List<string> idsBase = new List<string>();
                            List<Dictionary<string,object>> sTileOptionsBase = new List<Dictionary<string, object>>();
                            List<string> sTileEntityOptionsBase = new List<string>();

                            List<string> idsBackground = new List<string>();
                            List<Dictionary<string,object>> sTileOptionsBackground = new List<Dictionary<string, object>>();
                            List<string> sTileEntityOptionsBackground = new List<string>();
                            for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                                int xIndex = xStart+tileX;
                                int yIndex = yStart+tileY;
                                //Debug.Log("Chunk[" + chunkX + "," + chunkY + "], Partition[" + partitionX + "," + partitionY + "], index [" + xIndex + "," + yIndex + "]");
                                idsBase.Add(worldTileData.baseData.ids[xIndex][yIndex]);
                                sTileOptionsBase.Add(worldTileData.baseData.sTileOptions[xIndex][yIndex]);
                                sTileEntityOptionsBase.Add(worldTileData.baseData.sTileEntityOptions[xIndex][yIndex]);

                                idsBackground.Add(worldTileData.backgroundData.ids[xIndex][yIndex]);
                                sTileOptionsBackground.Add(worldTileData.backgroundData.sTileOptions[xIndex][yIndex]);
                                sTileEntityOptionsBackground.Add(worldTileData.backgroundData.sTileEntityOptions[xIndex][yIndex]);

                            }
                            partitionData.baseData.ids.Add(idsBase);
                            partitionData.baseData.sTileOptions.Add(sTileOptionsBase);
                            partitionData.baseData.sTileEntityOptions.Add(sTileEntityOptionsBase);

                            partitionData.backgroundData.ids.Add(idsBackground);
                            partitionData.backgroundData.sTileOptions.Add(sTileOptionsBackground);
                            partitionData.backgroundData.sTileEntityOptions.Add(sTileEntityOptionsBackground);
                        }
                        chunkPartitionDataList.Add(partitionData);
                    }
                }
                ChunkIO.writeNewChunk(new Vector2Int(chunkX,chunkY),-1,chunkPartitionDataList);
            }
        }
    }
}
