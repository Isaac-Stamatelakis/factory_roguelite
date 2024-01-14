using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ChunkIO {
    public static GameObject getChunkFromJson(Vector2Int chunkPosition, int dim, Transform closedSystemTransform) {
        string chunkName = "chunk[" + chunkPosition.x + "," + chunkPosition.y + "].json";
        string filePath = Application.dataPath + "/Resources/worlds/" + Global.WorldName + "/Chunks/dim" + dim + "/" + chunkName;
        string json = File.ReadAllText(filePath);
        Dictionary<string, object> dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>(json);
        JsonData jsonData = new JsonData();

        if (dict.ContainsKey("Type")) {
            jsonData.set("Type",dict["Type"].ToString());
        }
        if (dict.ContainsKey("TileBlocks")) {
            readSeralizedChunkTileData(jsonData,dict,"TileBlocks");
        }
        if (dict.ContainsKey("TileBackgrounds")) {
            readSeralizedChunkTileData(jsonData,dict,"TileBackgrounds");
        }
        if (dict.ContainsKey("TileObjects")) {
            readSeralizedChunkTileData(jsonData,dict,"TileObjects");
        }
        if (dict.ContainsKey("Entities")) {
            jsonData.set("Entities",Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntityData>>(dict["Entities"].ToString()));
        }
        if (dict.ContainsKey("EnergyConduits")) {
            readSeralizedChunkConduitData(jsonData,dict,"EnergyConduits");
        }
        if (dict.ContainsKey("ItemConduits")) {
            readSeralizedChunkConduitData(jsonData,dict,"ItemConduits");
        }
        if (dict.ContainsKey("FluidConduits")) {
            readSeralizedChunkConduitData(jsonData,dict,"FluidConduits");
        }
        if (dict.ContainsKey("SignalConduits")) {
            readSeralizedChunkConduitData(jsonData,dict,"SignalConduits");
        }
        
        if (jsonData == null) {
            Debug.LogError("Failed to import " + chunkName);
            return null;
        }
      
        GameObject chunkGameObject = new GameObject();
        switch ((string) jsonData.get("Type")) {
            case "Static":
                chunkGameObject.name = "static" + chunkName;
                StaticChunkProperties staticChunkProperties = chunkGameObject.AddComponent<StaticChunkProperties>();
                staticChunkProperties.initalize(dim,chunkPosition,jsonData,closedSystemTransform);
                
                break;
            case "Dynamic":
                chunkGameObject.name = "dynamic" + chunkName;
                DynamicChunkProperties dynamicChunkProperties = chunkGameObject.AddComponent<DynamicChunkProperties>();
                dynamicChunkProperties.initalize(dim,chunkPosition,jsonData,closedSystemTransform);
                break;
            case "DynamicConduit":
                chunkGameObject.name = "dynamicconduit" + chunkName;
                DynamicConduitChunkProperties dynamicConduitChunkProperties = chunkGameObject.AddComponent<DynamicConduitChunkProperties>();
                dynamicConduitChunkProperties.initalize(dim,chunkPosition,jsonData,closedSystemTransform);
                break;
        }
        return chunkGameObject;

        
    }

    private static void readSeralizedChunkTileData(JsonData jsonData,Dictionary<string,object> dict, string containerName) {
        jsonData.set(containerName,Newtonsoft.Json.JsonConvert.DeserializeObject<SeralizedChunkTileData>(dict[containerName].ToString()));
    }
    private static void readSeralizedChunkConduitData(JsonData jsonData,Dictionary<string,object> dict, string containerName) {
        jsonData.set(containerName,Newtonsoft.Json.JsonConvert.DeserializeObject<SeralizedChunkConduitData>(dict[containerName].ToString()));
    }
    public static void writeChunk(JsonData jsonData, ChunkProperties chunkProperties) {
        
        
        string filePath = Application.dataPath + "/Resources/worlds/"  + Global.WorldName + "/Chunks/dim" + chunkProperties.Dim + "/chunk[" + chunkProperties.ChunkPosition.x + "," + chunkProperties.ChunkPosition.y + "].json";
        
        
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonData.dict);
        System.IO.File.WriteAllText(filePath, json);
        
        
        
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
    public List<List<int>> ids;
    public List<List<Dictionary<string,object>>> sTileOptions;
    public List<List<Dictionary<string,object>>> sTileEntityOptions;
}

[System.Serializable]
public class SeralizedChunkConduitData {
    public List<List<int>> ids;
    public List<List<ConduitOptions>> conduitOptions;
}



