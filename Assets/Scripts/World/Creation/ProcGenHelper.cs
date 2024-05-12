using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.IO;
using System;
using Entities;

namespace WorldModule.Caves {
    public static class WorldGenerationFactory {
        public static void saveToJson(SerializedTileData worldTileData, Cave cave, int dim, string dimPath) {
            UnityEngine.Vector2Int caveSize = cave.getChunkCaveSize();
            IntervalVector caveCoveredArea = cave.getChunkCoveredArea();
            int tileMaxX = Global.ChunkSize*caveSize.x;
            int tileMaxY = Global.ChunkSize*caveSize.y;
            int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
            int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
            for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
                for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                    saveChunk(chunkX, chunkY,minX,minY,dim,worldTileData,dimPath);
                }
            }
        }

        public static void saveToJson(SerializedTileData worldTileData, Vector2Int caveSize, IntervalVector caveCoveredArea, int dim, string dimPath) {
            int tileMaxX = Global.ChunkSize*caveSize.x;
            int tileMaxY = Global.ChunkSize*caveSize.y;
            int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
            int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
            for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
                for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                    saveChunk(chunkX, chunkY,minX,minY,dim,worldTileData,dimPath);
                }
            }
        }

        private static void saveChunk(int chunkX, int chunkY, int minX, int minY, int dim, SerializedTileData worldTileData, string dimPath) {
            List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
            for (int partitionX = 0; partitionX < Global.PartitionsPerChunk; partitionX ++) {
                for (int partitionY = 0; partitionY < Global.PartitionsPerChunk; partitionY ++) {
                    chunkPartitionDataList.Add(convertPartition(chunkX,chunkY,minX,minY,partitionX,partitionY,worldTileData));
                }
            }
            ChunkIO.writeNewChunk(new Vector2Int(chunkX,chunkY),dim,chunkPartitionDataList,dimPath);
        }

        private static IChunkPartitionData convertPartition(int chunkX, int chunkY, int minX, int minY, int partitionX, int partitionY, SerializedTileData worldTileData) {
            int xStart = partitionX*Global.ChunkPartitionSize + Global.ChunkSize * (chunkX-minX);
            int yStart = partitionY*Global.ChunkPartitionSize + Global.ChunkSize * (chunkY-minY);
            
            // TODO ENTITIES
            List<SeralizedEntityData> entityData = new List<SeralizedEntityData>(); 
            
            SerializedBaseTileData baseData = new SerializedBaseTileData();
            baseData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            baseData.sTileOptions = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            baseData.sTileEntityOptions = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            SerializedBackgroundTileData backgroundData = new SerializedBackgroundTileData();
            backgroundData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            SeralizedFluidTileData fluidData = new SeralizedFluidTileData();
            fluidData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            fluidData.fill = new int[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            for (int tileX = 0; tileX < Global.ChunkPartitionSize; tileX ++) {
                for (int tileY = 0; tileY < Global.ChunkPartitionSize; tileY ++) {
                    int xIndex = xStart+tileX;
                    int yIndex = yStart+tileY;
                    baseData.ids[tileX,tileY] = worldTileData.baseData.ids[xIndex,yIndex];
                    baseData.sTileEntityOptions[tileX,tileY] = worldTileData.baseData.sTileEntityOptions[xIndex,yIndex];
                    baseData.sTileOptions[tileX,tileY] = worldTileData.baseData.sTileOptions[xIndex,yIndex];

                    backgroundData.ids[tileX,tileY] = worldTileData.backgroundData.ids[xIndex,yIndex];

                    fluidData.ids[tileX,tileY] = worldTileData.fluidData.ids[xIndex,yIndex];
                    fluidData.fill[tileX,tileY] = worldTileData.fluidData.fill[xIndex,yIndex];
                }
            }
            if (worldTileData is SerializedTileConduitData partionConduitData) {
                SeralizedChunkConduitData itemConduitData = new SeralizedChunkConduitData();
                itemConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                itemConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                SeralizedChunkConduitData fluidConduitData = new SeralizedChunkConduitData();
                fluidConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                fluidConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                SeralizedChunkConduitData energyConduitData = new SeralizedChunkConduitData();
                energyConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                energyConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];

                SeralizedChunkConduitData signalConduitData = new SeralizedChunkConduitData();
                signalConduitData.ids = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                signalConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                
                SeralizedChunkConduitData matrixConduitData = new SeralizedChunkConduitData();
                matrixConduitData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
                matrixConduitData.conduitOptions = new string[Global.ChunkPartitionSize, Global.ChunkPartitionSize];
                return new SerializedTileConduitData(
                    baseTileData: baseData,
                    backgroundTileData: backgroundData,
                    entityData: entityData,
                    fluidTileData: fluidData,
                    itemConduitData: itemConduitData,
                    fluidConduitData: fluidConduitData,
                    energyConduitData: energyConduitData,
                    signalConduitData: signalConduitData,
                    matrixConduitData: matrixConduitData
                );
            }
            return new SerializedTileData(
                baseTileData: baseData,
                backgroundTileData: backgroundData,
                entityData: entityData,
                fluidTileData: fluidData
            );
        }
    }

}
