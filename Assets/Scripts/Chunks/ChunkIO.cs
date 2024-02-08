using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChunkModule;

namespace ChunkModule.IO {
    public class ChunkIO {

        public static bool jsonExists(Vector2Int chunkPosition, int dim) {
            string chunkName = getName(chunkPosition);
            string filePath = getPath(chunkPosition,dim);
            return (Directory.Exists(filePath));
        }
        public static GameObject getChunkFromJson(Vector2Int chunkPosition, ClosedChunkSystem closedChunkSystem) {
            string chunkName = getName(chunkPosition);
            string filePath = getPath(chunkPosition,closedChunkSystem.Dim);
            string json = null;
            if (File.Exists(filePath))
            {
                json = File.ReadAllText(filePath);
            } else {
                /*
                if (closedChunkSystem is ConduitTileClosedChunkSystem) {
                    json = File.ReadAllText(Application.dataPath+"/Resources/Json/conduit_chunk_empty.json");
                } else if (closedChunkSystem is TileClosedChunkSystem) {
                    json = File.ReadAllText(Application.dataPath+"/Resources/Json/dynamic_chunk_empty.json");
                } 
                Debug.Log("Created new chunk " + chunkName + " dim " + closedChunkSystem.Dim);
                */
            }
            if (json == null) { 
                return null;
            }
            List<SerializedTileData> chunkPartitionDataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SerializedTileData>>(json);

            List<ChunkPartitionData> chunkPartitionData = new List<ChunkPartitionData>();
            foreach (SerializedTileData serializedTileData in chunkPartitionDataList) {
                chunkPartitionData.Add(serializedTileData);
            }
            GameObject chunkGameObject = new GameObject();
            chunkGameObject.name = chunkName;
            Chunk chunk = chunkGameObject.AddComponent<Chunk>();
            chunk.initalize(closedChunkSystem.Dim,chunkPartitionData,chunkPosition,closedChunkSystem);
            closedChunkSystem.addChunk(chunk);
            return chunkGameObject;

            
        }

        public static void writeChunk(IChunk chunk) {

            File.WriteAllText(ChunkIO.getPath(chunk),Newtonsoft.Json.JsonConvert.SerializeObject(chunk.getChunkPartitionData()));
        }
        public static string getPath(Vector2Int chunkPosition, int dim) {
            return Application.persistentDataPath + "/worlds/" + Global.WorldName + "/Chunks/dim" + dim + "/" + getName(chunkPosition);
        }

        public static string getPath(IChunk chunk) {
            return getPath(chunk.getPosition(),chunk.getDim());
        }
        public static string getName(Vector2Int chunkPosition) {
            return "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
        }

        public static void writeNewChunk(Vector2Int chunkPosition, int dim, List<ChunkPartitionData> data) {
            File.WriteAllText(ChunkIO.getPath(chunkPosition,dim),Newtonsoft.Json.JsonConvert.SerializeObject(data));
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
    public class SeralizedChunkTileData {
        public List<List<string>> ids;
        public List<List<Dictionary<string,object>>> sTileOptions;
        public List<List<string>> sTileEntityOptions;
    }

    [System.Serializable]
    public class SeralizedChunkConduitData {
        public List<List<string>> ids;
        public List<List<ConduitOptions>> conduitOptions;
    }
}



