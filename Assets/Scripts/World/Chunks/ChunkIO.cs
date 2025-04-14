using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chunks;
using Chunks.Systems;
using WorldModule;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Chunks.Partitions;
using Tiles;

namespace Chunks.IO {
    public class ChunkIO {
        public static List<SoftLoadedConduitTileChunk> GetUnloadedChunks(int dim, string path)
        {
            if (!Directory.Exists(path)) return new List<SoftLoadedConduitTileChunk>();
            string[] files = Directory.GetFiles(path);
            List<SoftLoadedConduitTileChunk> unloadedChunks = new List<SoftLoadedConduitTileChunk>();
            foreach (string file in files) {
                if (file.Contains(".meta")) continue;
                string[] seperated = file.Split("\\");
                string name = seperated[seperated.Length-1];
                string[] split = name.Split("[");
                string uncleanXY = split[1].Replace("]","").Replace(".json","");
                string[] xy = uncleanXY.Split(",");
                int x = Convert.ToInt32(xy[0]);
                int y = Convert.ToInt32(xy[1]);
                byte[] compressedData = File.ReadAllBytes(file);
                string json = WorldLoadUtils.DecompressString(compressedData);
                List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorldTileConduitData>>(json));
                SoftLoadedConduitTileChunk unloadedConduitTileChunk = new SoftLoadedConduitTileChunk(chunkPartitionDataList,new Vector2Int(x,y),dim);
                unloadedChunks.Add(unloadedConduitTileChunk);
            }
            return unloadedChunks;
        }

        public static ILoadedChunk GetChunkFromUnloadedChunk(SoftLoadedConduitTileChunk unloadedConduitTileChunk, ClosedChunkSystem closedChunkSystem) {
            string chunkName = GetName(unloadedConduitTileChunk.Position);
            GameObject chunkGameObject = new GameObject();
            chunkGameObject.name = chunkName;
            Chunk chunk = chunkGameObject.AddComponent<Chunk>();
            chunk.InitalizeFromUnloaded(closedChunkSystem.Dim,unloadedConduitTileChunk.Partitions,unloadedConduitTileChunk.Position,closedChunkSystem);
            return chunk;
        }
        public static ILoadedChunk GetChunkFromJson(Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            string chunkName = GetName(chunkPosition);
            string filePath = getPath(chunkPosition,closedChunkSystem.Dim);
            if (!File.Exists(filePath)) return null;
            byte[] compressed = File.ReadAllBytes(filePath);
            string json = WorldLoadUtils.DecompressString(compressed);
            List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
            if (closedChunkSystem is TileClosedChunkSystem) {
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<SeralizedWorldData>>(json));
            } else if (closedChunkSystem is ConduitTileClosedChunkSystem) {
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorldTileConduitData>>(json));
            }
            List<IChunkPartitionData> chunkPartitionData = new List<IChunkPartitionData>();
            foreach (IChunkPartitionData serializedTileData in chunkPartitionDataList) {
                chunkPartitionData.Add(serializedTileData);
            }
            GameObject chunkGameObject = new GameObject();
            chunkGameObject.name = chunkName;
            Chunk chunk = chunkGameObject.AddComponent<Chunk>();
            chunk.Initalize(closedChunkSystem.Dim,chunkPartitionData,chunkPosition,closedChunkSystem);
            return chunk;

            
        }

        public static void WriteChunk(IChunk chunk, string path = null, bool directory = false) {
            if (path == null) {
                path = ChunkIO.GetPath(chunk);
            }   
            if (directory) {
                path = Path.Combine(path,GetName(chunk.GetPosition()));
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(chunk.GetChunkPartitionData());
            byte[] compressed = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path,compressed);
        }
        
        public static string getPath(Vector2Int chunkPosition, int dim) {
            return Path.Combine(WorldLoadUtils.GetDimPath(dim),GetName(chunkPosition));
        }

        public static string GetPath(IChunk chunk) {
            return getPath(chunk.GetPosition(),chunk.GetDim());
        }
        public static string GetName(Vector2Int chunkPosition) {
            return "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
        }

        public static void WriteNewChunk(Vector2Int chunkPosition, int dim, List<IChunkPartitionData> data, string dimPath) {
            string chunkPath = Path.Combine(dimPath,GetName(chunkPosition));
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(chunkPath,json);
        }

        private static List<List<Dictionary<string,object>>> getSeralizedOptions(string tilePath, string json, string optionPath) {
            JObject tileJObject = (JObject) JObject.Parse(json)[tilePath];
            List<List<Dictionary<string,object>>> dictionaries = JsonConvert.DeserializeObject<List<List<Dictionary<string,object>>>>(tileJObject[optionPath].ToString());
            return dictionaries;
            
        }


        private static void printArray(int[] array) {
            string printString = "";
            for (int n = 0; n < array.Length; n ++) {
                printString += array[n] + ", ";
            }
            Debug.Log(printString);
        }

    }

    [System.Serializable]
    public class SerializedBaseTileData {
        public string[,] ids;
        public BaseTileData[,] sTileOptions;
        public string[,] sTileEntityOptions;
    }

    public class SeralizedFluidTileData {
        public string[,] ids;
        public float[,] fill;
    }

    [System.Serializable]
    public class SerializedBackgroundTileData {
        public string[,] ids;
    }

    [System.Serializable]
    public class SeralizedChunkConduitData {
        public string[,] ids;
        public string[,] conduitOptions;
    }

    public static class SerializedTileDataFactory {
        public static SeralizedFluidTileData createEmptyFluidData(int width, int height) {
            SeralizedFluidTileData seralizedFluidTileData = new SeralizedFluidTileData();
            seralizedFluidTileData.ids = new string[width,height];
            seralizedFluidTileData.fill = new float[width,height];
            return seralizedFluidTileData;
        }

        public static SeralizedChunkConduitData createEmptyConduitData(int width, int height) {
            SeralizedChunkConduitData seralizedChunkConduitData = new SeralizedChunkConduitData();
            seralizedChunkConduitData.ids = new string[width,height];
            seralizedChunkConduitData.conduitOptions = new string[width,height];
            return seralizedChunkConduitData;
        }

        public static SerializedBackgroundTileData createEmptyBackgroundData(int width, int height) {
            SerializedBackgroundTileData serializedBackgroundTileData = new SerializedBackgroundTileData();
            serializedBackgroundTileData.ids = new string[width,height];
            return serializedBackgroundTileData;
        }

        public static SerializedBaseTileData createEmptyBaseData(int width, int height) {
            SerializedBaseTileData serializedBaseTileData = new SerializedBaseTileData();
            serializedBaseTileData.ids = new string[width,height];
            serializedBaseTileData.sTileEntityOptions = new string[width,height];
            serializedBaseTileData.sTileOptions = new BaseTileData[width,height];
            return serializedBaseTileData;
        }
    }
}



