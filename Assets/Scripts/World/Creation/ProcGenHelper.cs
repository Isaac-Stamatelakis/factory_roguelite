using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.IO;
using System;
using Entities;
using Chunks.Partitions;
using Chunks;
using DevTools.Structures;
using Newtonsoft.Json;
using TileEntity.Instances.Creative.CreativeChest;
using Tiles;
using Tiles.TileMap.Interval;
using WorldModule.Caves;

namespace WorldModule {
    public static class WorldGenerationFactory {
        public static void SaveToJson(SeralizedWorldData worldTileData, CaveInstance cave, int dim, string dimPath) {
            UnityEngine.Vector2Int caveSize = cave.CaveObject.GetChunkCaveSize();
            IntervalVector caveCoveredArea = cave.CaveObject.GetChunkCoveredArea();
            int tileMaxX = Global.CHUNK_SIZE*caveSize.x;
            int tileMaxY = Global.CHUNK_SIZE*caveSize.y;
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
        
        public static IEnumerator SaveToJsonCoroutine(SeralizedWorldData worldTileData, Vector2Int caveSize, int dim, string dimPath) {
            // Normalize coordinates so center is at 0,0
            int minX = -(caveSize.x-1)/2;
            int maxX = (caveSize.x)/2;
            int minY = -(caveSize.y-1)/2;
            int maxY = (caveSize.y)/2;
            int saves = 0;
            var delay = new WaitForFixedUpdate();
            const int DELAYS_PER_SAVES = 2;
            for (int chunkY = minY; chunkY <= maxY; chunkY ++) {
                for (int chunkX = minX; chunkX <= maxX; chunkX ++) {
                    SaveChunk(chunkX, chunkY, minX, minY, dim, worldTileData, dimPath);
                    saves++;
                    if (saves > DELAYS_PER_SAVES)
                    {
                        saves = 0;
                        yield return delay;
                    }
                }
            }
        }

        public static void SaveChunk(int chunkX, int chunkY, int minX, int minY, int dim, SeralizedWorldData worldTileData, string dimPath) {
            List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
            for (int partitionX = 0; partitionX < Global.PARTITIONS_PER_CHUNK; partitionX ++) {
                for (int partitionY = 0; partitionY < Global.PARTITIONS_PER_CHUNK; partitionY ++) {
                    chunkPartitionDataList.Add(ConvertPartition(chunkX,chunkY,minX,minY,partitionX,partitionY,worldTileData));
                    
                }
            }

            SoftLoadedConduitTileChunk softLoadedChunk = new SoftLoadedConduitTileChunk(chunkPartitionDataList, new Vector2Int(chunkX, chunkY), dim);
            ChunkIO.WriteChunk(softLoadedChunk, path: dimPath,directory:true);
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
            string fromBaseId = copyFrom.baseData.ids[positionFrom.x,positionFrom.y];
            if (fromBaseId != StructureGeneratorHelper.FILL_ID)
            {
                copyTo.baseData.sTileEntityOptions[positionTo.x,positionTo.y] = copyFrom.baseData.sTileEntityOptions[positionFrom.x,positionFrom.y];
                copyTo.baseData.sTileOptions[positionTo.x,positionTo.y] = copyFrom.baseData.sTileOptions[positionFrom.x,positionFrom.y];
                copyTo.baseData.ids[positionTo.x, positionTo.y] = fromBaseId;
            }
           
            copyTo.backgroundData.ids[positionTo.x,positionTo.y] = copyFrom.backgroundData.ids[positionFrom.x,positionFrom.y];

            copyTo.fluidData.ids[positionTo.x,positionTo.y] = copyFrom.fluidData.ids[positionFrom.x,positionFrom.y];
            copyTo.fluidData.fill[positionTo.x,positionTo.y] = copyFrom.fluidData.fill[positionFrom.x,positionFrom.y];
        }
        

        private static IChunkPartitionData ConvertPartition(int chunkX, int chunkY, int minX, int minY, int partitionX, int partitionY, SeralizedWorldData worldTileData) {
            int xStart = partitionX*Global.CHUNK_PARTITION_SIZE + Global.CHUNK_SIZE * (chunkX-minX);
            int yStart = partitionY*Global.CHUNK_PARTITION_SIZE + Global.CHUNK_SIZE * (chunkY-minY);
            float worldStartX = (partitionX*Global.CHUNK_PARTITION_SIZE + Global.CHUNK_SIZE * chunkX) * Global.TILE_SIZE;
            float worldStartY = (partitionY*Global.CHUNK_PARTITION_SIZE + Global.CHUNK_SIZE * chunkY) * Global.TILE_SIZE;
            
            List<SeralizedEntityData> entityDataList = new List<SeralizedEntityData>();
            for (var index = worldTileData.entityData.Count-1; index >= 0 ; index--)
            {
                var entityData = worldTileData.entityData[index];
                if (
                    entityData.x >= worldStartX &&
                    entityData.y >= worldStartY &&
                    entityData.x < worldStartX + Global.CHUNK_PARTITION_SIZE * Global.TILE_SIZE &&
                    entityData.y < worldStartY + Global.CHUNK_PARTITION_SIZE * Global.TILE_SIZE 
                )
                {
                    entityDataList.Add(entityData);
                    worldTileData.entityData.RemoveAt(index);
                }
            }

            SerializedBaseTileData baseData = new SerializedBaseTileData();
            baseData.ids = new string[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];
            baseData.sTileOptions = new BaseTileData[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];
            baseData.sTileEntityOptions = new string[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];

            SerializedBackgroundTileData backgroundData = new SerializedBackgroundTileData();
            backgroundData.ids = new string[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];

            SeralizedFluidTileData fluidData = new SeralizedFluidTileData();
            fluidData.ids = new string[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];
            fluidData.fill = new float[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];

            for (int tileX = 0; tileX < Global.CHUNK_PARTITION_SIZE; tileX ++) {
                for (int tileY = 0; tileY < Global.CHUNK_PARTITION_SIZE; tileY ++) {
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
            if (worldTileData is WorldTileConduitData) {
                SeralizedChunkConduitData itemConduitData = new SeralizedChunkConduitData();
                itemConduitData.ids = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
                itemConduitData.conduitOptions = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];

                SeralizedChunkConduitData fluidConduitData = new SeralizedChunkConduitData();
                fluidConduitData.ids = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
                fluidConduitData.conduitOptions = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];

                SeralizedChunkConduitData energyConduitData = new SeralizedChunkConduitData();
                energyConduitData.ids = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
                energyConduitData.conduitOptions = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];

                SeralizedChunkConduitData signalConduitData = new SeralizedChunkConduitData();
                signalConduitData.ids = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
                signalConduitData.conduitOptions = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
                
                SeralizedChunkConduitData matrixConduitData = new SeralizedChunkConduitData();
                matrixConduitData.ids = new string[Global.CHUNK_PARTITION_SIZE,Global.CHUNK_PARTITION_SIZE];
                matrixConduitData.conduitOptions = new string[Global.CHUNK_PARTITION_SIZE, Global.CHUNK_PARTITION_SIZE];
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
