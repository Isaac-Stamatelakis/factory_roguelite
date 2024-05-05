using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;
using System;

namespace WorldModule.Generation {
    public static class WorldGenerationFactory {
        public static void saveToJson(WorldTileData worldTileData, GeneratedArea cave, int dim) {
            UnityEngine.Vector2Int caveSize = cave.getChunkCaveSize();
            IntervalVector caveCoveredArea = cave.getChunkCoveredArea();
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

        public static void saveToJson(WorldTileData worldTileData, Vector2Int caveSize, IntervalVector caveCoveredArea, int dim) {
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
            
            partitionData.baseData = new SerializedBaseTileData();
            partitionData.baseData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            partitionData.baseData.sTileOptions = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            partitionData.baseData.sTileEntityOptions = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            partitionData.backgroundData = new SerializedBackgroundTileData();
            partitionData.backgroundData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            partitionData.fluidData = new SeralizedFluidTileData();
            partitionData.fluidData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            partitionData.fluidData.fill = new int[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
                for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                    int xIndex = xStart+tileX;
                    int yIndex = yStart+tileY;
                    partitionData.baseData.ids[tileX,tileY] = worldTileData.baseData.ids[xIndex,yIndex];
                    partitionData.baseData.sTileEntityOptions[tileX,tileY] = worldTileData.baseData.sTileEntityOptions[xIndex,yIndex];
                    partitionData.baseData.sTileOptions[tileX,tileY] = worldTileData.baseData.sTileOptions[xIndex,yIndex];

                    partitionData.backgroundData.ids[tileX,tileY] = worldTileData.backgroundData.ids[xIndex,yIndex];

                    partitionData.fluidData.ids[tileX,tileY] = worldTileData.fluidData.ids[xIndex,yIndex];
                    partitionData.fluidData.fill[tileX,tileY] = worldTileData.fluidData.fill[xIndex,yIndex];
                }
            }
            if (worldTileData is WorldTileConduitData) {
                SerializedTileConduitData partionConduitData = (SerializedTileConduitData) partitionData;
                WorldTileConduitData tileConduitData = (WorldTileConduitData) worldTileData;

                partionConduitData.itemConduitData = new SeralizedChunkConduitData();
                partionConduitData.itemConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                partionConduitData.itemConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                partionConduitData.fluidConduitData = new SeralizedChunkConduitData();
                partionConduitData.fluidConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                partionConduitData.fluidConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                partionConduitData.energyConduitData = new SeralizedChunkConduitData();
                partionConduitData.energyConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                partionConduitData.energyConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                partionConduitData.signalConduitData = new SeralizedChunkConduitData();
                partionConduitData.signalConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                partionConduitData.signalConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                
                partionConduitData.matrixConduitData = new SeralizedChunkConduitData();
                partionConduitData.matrixConduitData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
                partionConduitData.matrixConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
            }
            return partitionData;
        }
    }

}
