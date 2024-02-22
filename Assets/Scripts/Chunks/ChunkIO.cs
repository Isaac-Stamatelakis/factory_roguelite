using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChunkModule;
using UI.Title;
using ChunkModule.ClosedChunkSystemModule;

namespace ChunkModule.IO {
    public class ChunkIO {

        public static bool jsonExists(Vector2Int chunkPosition, int dim) {
            string chunkName = getName(chunkPosition);
            string filePath = getPath(chunkPosition,dim);
            return (Directory.Exists(filePath));
        }
        public static IChunk getChunkFromJson(Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
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
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<SerializedTileData>>(json));
            } else if (closedChunkSystem is ConduitTileClosedChunkSystem) {
                chunkPartitionDataList.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<List<SerializedTileConduitData>>(json));
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
            return Application.persistentDataPath + "/worlds/" + Global.WorldName + "/Dimensions/dim" + dim + "/" + getName(chunkPosition);
        }

        public static string getPath(IChunk chunk) {
            return getPath(chunk.getPosition(),chunk.getDim());
        }
        public static string getName(Vector2Int chunkPosition) {
            return "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
        }

        public static void writeNewChunk(Vector2Int chunkPosition, int dim, List<IChunkPartitionData> data) {
            File.WriteAllText(getPath(chunkPosition,dim),Newtonsoft.Json.JsonConvert.SerializeObject(data));
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



