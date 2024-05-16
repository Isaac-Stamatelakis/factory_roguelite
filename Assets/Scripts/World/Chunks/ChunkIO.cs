using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Chunks;
using UI.Title;
using Chunks.ClosedChunkSystemModule;
using WorldModule;
using System;
using Chunks.Partitions;

namespace Chunks.IO {
    public class ChunkIO {

        public static bool jsonExists(Vector2Int chunkPosition, int dim) {
            string chunkName = getName(chunkPosition);
            string filePath = getPath(chunkPosition,dim);
            return (Directory.Exists(filePath));
        }
        public static List<SoftLoadedConduitTileChunk> getUnloadedChunks(int dim, string path) {
            string[] files = Directory.GetFiles(path);
            List<SoftLoadedConduitTileChunk> unloadedChunks = new List<SoftLoadedConduitTileChunk>();
            foreach (string file in files) {
                string[] seperated = file.Split("\\");
                string name = seperated[seperated.Length-1];
                string[] split = name.Split("[");
                string uncleanXY = split[1].Replace("]","").Replace(".json","");
                string[] xy = uncleanXY.Split(",");
                int x = Convert.ToInt32(xy[0]);
                int y = Convert.ToInt32(xy[1]);
                string data = File.ReadAllText(file);
                List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorldTileConduitData>>(data));
                SoftLoadedConduitTileChunk unloadedConduitTileChunk = new SoftLoadedConduitTileChunk(chunkPartitionDataList,new Vector2Int(x,y),dim);
                unloadedChunks.Add(unloadedConduitTileChunk);
            }
            return unloadedChunks;
        }

        public static SoftLoadedConduitTileChunk fromData(string data, Vector2Int position, int dim) {
            List<IChunkPartitionData> chunkPartitionDataList = new List<IChunkPartitionData>();
            chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<WorldTileConduitData>>(data));
            SoftLoadedConduitTileChunk unloadedConduitTileChunk = new SoftLoadedConduitTileChunk(chunkPartitionDataList,position,dim);
            return unloadedConduitTileChunk;
        }

        public static ILoadedChunk getChunkFromUnloadedChunk(SoftLoadedConduitTileChunk unloadedConduitTileChunk, ClosedChunkSystem closedChunkSystem) {
            string chunkName = getName(unloadedConduitTileChunk.Position);
            GameObject chunkGameObject = new GameObject();
            chunkGameObject.name = chunkName;
            Chunk chunk = chunkGameObject.AddComponent<Chunk>();
            chunk.initalizeFromUnloaded(closedChunkSystem.Dim,unloadedConduitTileChunk.Partitions,unloadedConduitTileChunk.Position,closedChunkSystem);
            return chunk;
        }
        public static ILoadedChunk getChunkFromJson(Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            string chunkName = getName(chunkPosition);
            string filePath = getPath(chunkPosition,closedChunkSystem.Dim);
            string json = null;
            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
            } else {
                return null;
            }
            if (json == null) { 
                return null;
            }
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
            chunk.initalize(closedChunkSystem.Dim,chunkPartitionData,chunkPosition,closedChunkSystem);
            return chunk;

            
        }

        public static void writeChunk(IChunk chunk) {

            File.WriteAllText(ChunkIO.getPath(chunk),Newtonsoft.Json.JsonConvert.SerializeObject(chunk.getChunkPartitionData()));
        }
        public static string getPath(Vector2Int chunkPosition, int dim) {
            return Path.Combine(WorldLoadUtils.getDimPath(dim),getName(chunkPosition));
        }

        public static string getPath(IChunk chunk) {
            return getPath(chunk.getPosition(),chunk.getDim());
        }
        public static string getName(Vector2Int chunkPosition) {
            return "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
        }

        public static void writeNewChunk(Vector2Int chunkPosition, int dim, List<IChunkPartitionData> data, string dimPath) {
            string chunkPath = Path.Combine(dimPath,getName(chunkPosition));
            File.WriteAllText(chunkPath,Newtonsoft.Json.JsonConvert.SerializeObject(data));
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
        public string[,] sTileOptions;
        public string[,] sTileEntityOptions;
    }

    public class SeralizedFluidTileData {
        public string[,] ids;
        public int[,] fill;
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
}



