using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;
using System;

public static class ProcGenHelper {
    public static void saveToJson(WorldTileData worldTileData, Cave cave, int dim) {
        UnityEngine.Vector2Int caveSize = cave.getChunkDimensions();
        IntervalVector caveCoveredArea = cave.getCoveredArea();
        int tileMaxX = Global.ChunkSize*caveSize.x;
        int tileMaxY = Global.ChunkSize*caveSize.y;
        int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
        int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
        for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
            for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                saveChunk(chunkX, chunkY,minX,minY,dim,worldTileData);
            }
        }
    }

    private static void saveChunk(int chunkX, int chunkY, int minX, int minY, int dim, WorldTileData worldTileData) {
        List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
        for (int partitionX = 0; partitionX < Global.PartitionsPerChunk; partitionX ++) {
            for (int partitionY = 0; partitionY < Global.PartitionsPerChunk; partitionY ++) {
                chunkPartitionDataList.Add(convertPartition(chunkX,chunkY,minX,minY,partitionX,partitionY,worldTileData));
            }
        }
        ChunkIO.writeNewChunk(new Vector2Int(chunkX,chunkY),dim,chunkPartitionDataList);
    }

    private static IChunkPartitionData convertPartition(int chunkX, int chunkY, int minX, int minY, int partitionX, int partitionY, WorldTileData worldTileData) {
        int xStart = partitionX*Global.ChunkPartitionSize + Global.ChunkSize * (chunkX-minX);
        int yStart = partitionY*Global.ChunkPartitionSize + Global.ChunkSize * (chunkY-minY);
        SerializedTileData partitionData;
        if (worldTileData is WorldTileConduitData) {
            partitionData = new SerializedTileConduitData();
        } else {
            partitionData = new SerializedTileData();
        }
        // TODO ENTITIES
        partitionData.entityData = new List<EntityData>(); 
        
        partitionData.baseData = new SeralizedChunkTileData();
        partitionData.baseData.ids = new List<List<string>>();
        partitionData.baseData.sTileOptions = new List<List<string>>();
        partitionData.baseData.sTileEntityOptions = new List<List<string>>();

        partitionData.backgroundData = new SeralizedChunkTileData();
        partitionData.backgroundData.ids = new List<List<string>>();
        partitionData.backgroundData.sTileOptions = new List<List<string>>();
        partitionData.backgroundData.sTileEntityOptions = new List<List<string>>();

        for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
            List<string> idsBase = new List<string>();
            List<string> sTileOptionsBase = new List<string>();
            List<string> sTileEntityOptionsBase = new List<string>();

            List<string> idsBackground = new List<string>();
            List<string> sTileOptionsBackground = new List<string>();
            List<string> sTileEntityOptionsBackground = new List<string>();
            for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                int xIndex = xStart+tileX;
                int yIndex = yStart+tileY;
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
        if (worldTileData is WorldTileConduitData) {
            SerializedTileConduitData partionConduitData = (SerializedTileConduitData) partitionData;
            WorldTileConduitData tileConduitData = (WorldTileConduitData) worldTileData;

            partionConduitData.itemConduitData = new SeralizedChunkConduitData();
            partionConduitData.itemConduitData.ids = new List<List<string>>();
            partionConduitData.itemConduitData.conduitOptions = new List<List<string>>();

            partionConduitData.fluidConduitData = new SeralizedChunkConduitData();
            partionConduitData.fluidConduitData.ids = new List<List<string>>();
            partionConduitData.fluidConduitData.conduitOptions = new List<List<string>>();

            partionConduitData.energyConduitData = new SeralizedChunkConduitData();
            partionConduitData.energyConduitData.ids = new List<List<string>>();
            partionConduitData.energyConduitData.conduitOptions = new List<List<string>>();

            partionConduitData.signalConduitData = new SeralizedChunkConduitData();
            partionConduitData.signalConduitData.ids = new List<List<string>>();
            partionConduitData.signalConduitData.conduitOptions = new List<List<string>>();
            for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
                List<string> itemConduitIDs = new List<string>();
                List<string> itemConduitOptions = new List<string>();

                List<string> fluidConduitIDs = new List<string>();
                List<string> fluidConduitOptions = new List<string>();

                List<string> energyConduitIDs = new List<string>();
                List<string> energyConduitOptions = new List<string>();

                List<string> signalConduitIDs = new List<string>();
                List<string> signalConduitOptions = new List<string>();
                for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                    int xIndex = xStart+tileX;
                    int yIndex = yStart+tileY;

                    itemConduitIDs.Add(tileConduitData.itemConduitData.ids[xIndex][yIndex]);
                    itemConduitOptions.Add(tileConduitData.itemConduitData.conduitOptions[xIndex][yIndex]);

                    fluidConduitIDs.Add(tileConduitData.signalConduitData.ids[xIndex][yIndex]);
                    fluidConduitOptions.Add(tileConduitData.signalConduitData.conduitOptions[xIndex][yIndex]);

                    energyConduitIDs.Add(tileConduitData.energyConduitData.ids[xIndex][yIndex]);
                    energyConduitOptions.Add(tileConduitData.energyConduitData.conduitOptions[xIndex][yIndex]);

                    signalConduitIDs.Add(tileConduitData.signalConduitData.ids[xIndex][yIndex]);
                    signalConduitOptions.Add(tileConduitData.signalConduitData.conduitOptions[xIndex][yIndex]);
                    
                }
                partionConduitData.itemConduitData.ids.Add(itemConduitIDs);
                partionConduitData.itemConduitData.conduitOptions.Add(itemConduitOptions);

                partionConduitData.fluidConduitData.ids.Add(fluidConduitIDs);
                partionConduitData.fluidConduitData.conduitOptions.Add(fluidConduitOptions);

                partionConduitData.energyConduitData.ids.Add(fluidConduitIDs);
                partionConduitData.energyConduitData.conduitOptions.Add(fluidConduitOptions);

                partionConduitData.signalConduitData.ids.Add(signalConduitIDs);
                partionConduitData.signalConduitData.conduitOptions.Add(signalConduitOptions);
            }
        }
        return partitionData;
    }
}
