using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;
using System;
using Entities;
using Chunks.Partitions;
using Chunks;
using Tiles;
using WorldModule.Caves;

namespace WorldModule {
    public static class WorldGenerationFactory {
        public static void SaveToJson(SeralizedWorldData worldTileData, CaveInstance cave, int dim, string dimPath) {
            UnityEngine.Vector2Int caveSize = cave.getChunkCaveSize();
            IntervalVector caveCoveredArea = cave.getChunkCoveredArea();
            int tileMaxX = Global.ChunkSize*caveSize.x;
            int tileMaxY = Global.ChunkSize*caveSize.y;
            int minX = caveCoveredArea.X.LowerBound; int maxX = caveCoveredArea.X.UpperBound;
            int minY = caveCoveredArea.Y.LowerBound; int maxY = caveCoveredArea.Y.UpperBound;
            for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
                for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                    SaveChunk(chunkX, chunkY,minX,minY,dim,worldTileData,dimPath);
                }
            }
        }

        public static void SaveToJson(SeralizedWorldData worldTileData, Vector2Int caveSize, int dim, string dimPath) {
            // Normalize coordinates so center is at 0,0
            int minX = -(caveSize.x-1)/2;
            int maxX = (caveSize.x)/2;
            int minY = -(caveSize.y-1)/2;
            int maxY = (caveSize.y)/2;
            for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
                for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                    SaveChunk(chunkX, chunkY, minX, minY, dim, worldTileData, dimPath);
                }
            }
        }

        public static void SaveChunk(int chunkX, int chunkY, int minX, int minY, int dim, SeralizedWorldData worldTileData, string dimPath) {
            List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
            for (int partitionX = 0; partitionX < Global.PartitionsPerChunk; partitionX ++) {
                for (int partitionY = 0; partitionY < Global.PartitionsPerChunk; partitionY ++) {
                    chunkPartitionDataList.Add(ConvertPartition(chunkX,chunkY,minX,minY,partitionX,partitionY,worldTileData));
                }
            }
            ChunkIO.writeNewChunk(new Vector2Int(chunkX,chunkY),dim,chunkPartitionDataList,dimPath);
        }

        public static WorldTileConduitData CreateEmpty(Vector2Int size) {
            SerializedBaseTileData baseTileData = new SerializedBaseTileData();
            baseTileData.ids = new string[size.x,size.y];
            baseTileData.sTileEntityOptions = new string[size.x,size.y];
            baseTileData.sTileOptions = new BaseTileData[size.x,size.y];

            SerializedBackgroundTileData backgroundTileData = new SerializedBackgroundTileData();
            backgroundTileData.ids = new string[size.x,size.y];

            SeralizedFluidTileData seralizedFluidTileData = new SeralizedFluidTileData();
            seralizedFluidTileData.ids = new string[size.x,size.y];
            seralizedFluidTileData.fill = new float[size.x,size.y];

            SeralizedChunkConduitData itemConduitData = new SeralizedChunkConduitData();
            itemConduitData.ids = new string[size.x,size.y];
            itemConduitData.conduitOptions = new string[size.x,size.y];

            SeralizedChunkConduitData fluidConduitData = new SeralizedChunkConduitData();
            fluidConduitData.ids = new string[size.x,size.y];
            fluidConduitData.conduitOptions = new string[size.x,size.y];

            SeralizedChunkConduitData energyConduitData = new SeralizedChunkConduitData();
            energyConduitData.ids = new string[size.x,size.y];
            energyConduitData.conduitOptions = new string[size.x,size.y];

            SeralizedChunkConduitData signalConduitData = new SeralizedChunkConduitData();
            signalConduitData.ids = new string[size.x,size.y];
            signalConduitData.conduitOptions = new string[size.x,size.y];

            SeralizedChunkConduitData matrixConduitData = new SeralizedChunkConduitData();
            matrixConduitData.ids = new string[size.x,size.y];
            matrixConduitData.conduitOptions = new string[size.x,size.y];

            return new WorldTileConduitData(
                baseTileData: baseTileData,
                backgroundTileData: backgroundTileData,
                entityData: new List<SeralizedEntityData>(),
                fluidTileData: seralizedFluidTileData,
                itemConduitData: itemConduitData,
                fluidConduitData: fluidConduitData,
                energyConduitData: energyConduitData,
                signalConduitData: signalConduitData,
                matrixConduitData: matrixConduitData
            );
        }

        public static void MapWorldTileConduitData(WorldTileConduitData copyTo, WorldTileConduitData copyFrom, Vector2Int positionTo, Vector2Int positionFrom) {
            MapWorldTileData(copyTo,copyFrom,positionTo,positionFrom);
            copyTo.itemConduitData.ids[positionTo.x,positionTo.y] = copyFrom.itemConduitData.ids[positionFrom.x,positionFrom.y];
            copyTo.itemConduitData.conduitOptions[positionTo.x,positionTo.y] = copyFrom.itemConduitData.conduitOptions[positionFrom.x,positionFrom.y];

            copyTo.fluidConduitData.ids[positionTo.x,positionTo.y] = copyFrom.fluidConduitData.ids[positionFrom.x,positionFrom.y];
            copyTo.fluidConduitData.conduitOptions[positionTo.x,positionTo.y] = copyFrom.fluidConduitData.conduitOptions[positionFrom.x,positionFrom.y];

            copyTo.energyConduitData.ids[positionTo.x,positionTo.y] = copyFrom.energyConduitData.ids[positionFrom.x,positionFrom.y];
            copyTo.energyConduitData.conduitOptions[positionTo.x,positionTo.y] = copyFrom.energyConduitData.conduitOptions[positionFrom.x,positionFrom.y];

            copyTo.signalConduitData.ids[positionTo.x,positionTo.y] = copyFrom.signalConduitData.ids[positionFrom.x,positionFrom.y];
            copyTo.signalConduitData.conduitOptions[positionTo.x,positionTo.y] = copyFrom.signalConduitData.conduitOptions[positionFrom.x,positionFrom.y];

            copyTo.matrixConduitData.ids[positionTo.x,positionTo.y] = copyFrom.matrixConduitData.ids[positionFrom.x,positionFrom.y];
            copyTo.matrixConduitData.conduitOptions[positionTo.x,positionTo.y] = copyFrom.matrixConduitData.conduitOptions[positionFrom.x,positionFrom.y];
            
        }

        public static void MapWorldTileData(SeralizedWorldData copyTo, SeralizedWorldData copyFrom, Vector2Int positionTo, Vector2Int positionFrom) {
            copyTo.baseData.ids[positionTo.x,positionTo.y] = copyFrom.baseData.ids[positionFrom.x,positionFrom.y];
            copyTo.baseData.sTileEntityOptions[positionTo.x,positionTo.y] = copyFrom.baseData.sTileEntityOptions[positionFrom.x,positionFrom.y];
            copyTo.baseData.sTileOptions[positionTo.x,positionTo.y] = copyFrom.baseData.sTileOptions[positionFrom.x,positionFrom.y];

            copyTo.backgroundData.ids[positionTo.x,positionTo.y] = copyFrom.backgroundData.ids[positionFrom.x,positionFrom.y];

            copyTo.fluidData.ids[positionTo.x,positionTo.y] = copyFrom.fluidData.ids[positionFrom.x,positionFrom.y];
            copyTo.fluidData.fill[positionTo.x,positionTo.y] = copyFrom.fluidData.fill[positionFrom.x,positionFrom.y];
        }

        private static IChunkPartitionData ConvertPartition(int chunkX, int chunkY, int minX, int minY, int partitionX, int partitionY, SeralizedWorldData worldTileData) {
            int xStart = partitionX*Global.ChunkPartitionSize + Global.ChunkSize * (chunkX-minX);
            int yStart = partitionY*Global.ChunkPartitionSize + Global.ChunkSize * (chunkY-minY);
        
            List<SeralizedEntityData> entityDataList = new List<SeralizedEntityData>(); 
            foreach (SeralizedEntityData entityData in worldTileData.entityData) {
                if (
                    entityData.x >= xStart && 
                    entityData.y >= yStart && 
                    entityData.x < xStart + Global.ChunkPartitionSize && 
                    entityData.y < yStart + Global.ChunkPartitionSize
                ) {
                    Debug.Log($"{entityData.x},{entityData.y}");
                    entityDataList.Add(entityData);
                    
                }
            }
            if (entityDataList.Count > 0) {
                Debug.Log(entityDataList.Count);
            }
            SerializedBaseTileData baseData = new SerializedBaseTileData();
            baseData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            baseData.sTileOptions = new BaseTileData[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            baseData.sTileEntityOptions = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            SerializedBackgroundTileData backgroundData = new SerializedBackgroundTileData();
            backgroundData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

            SeralizedFluidTileData fluidData = new SeralizedFluidTileData();
            fluidData.ids = new string[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
            fluidData.fill = new float[Global.ChunkPartitionSize,Global.ChunkPartitionSize];

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
            if (worldTileData is WorldTileConduitData partionConduitData) {
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
                return new WorldTileConduitData(
                    baseTileData: baseData,
                    backgroundTileData: backgroundData,
                    entityData: entityDataList,
                    fluidTileData: fluidData,
                    itemConduitData: itemConduitData,
                    fluidConduitData: fluidConduitData,
                    energyConduitData: energyConduitData,
                    signalConduitData: signalConduitData,
                    matrixConduitData: matrixConduitData
                );
            }
            return new SeralizedWorldData(
                baseTileData: baseData,
                backgroundTileData: backgroundData,
                entityData: entityDataList,
                fluidTileData: fluidData
            );
        }
    }

}
