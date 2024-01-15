using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class IdDataMap
{
    private Dictionary<int, IdData> idMap;
    private static IdDataMap idDataMap;

    protected IdDataMap() {
        string filePath = Application.dataPath + "/Resources/Json/IdMap.json";
        string json = File.ReadAllText(filePath);
        Newtonsoft.Json.Linq.JArray array = (Newtonsoft.Json.Linq.JArray) Newtonsoft.Json.Linq.JObject.Parse(json)["idData"];
        idMap = new Dictionary<int, IdData>();
        foreach (Newtonsoft.Json.Linq.JObject jsonObject in array) {
            int id = (int) jsonObject["id"];
            string spritePath = (string) jsonObject["spritePath"];
            string name = (string) jsonObject["name"];
            string dataType = (string) jsonObject["dataType"];
            string tileEntityPath = null;
            if (jsonObject.ContainsKey("tileEntityPath")) {
                tileEntityPath = (string) jsonObject["tileEntityPath"];
            }
            switch (dataType) {
                case "Tile":
                    string tileType = (string) jsonObject["tileType"];
                    SDictionary tileOptions = JsonConvert.DeserializeObject<SDictionary>(jsonObject["tileOptions"].ToString());
                    TileData tileData = new TileData {
                        id = id,
                        spritePath = spritePath,
                        name = name,
                        dataType = dataType,
                        tileType = tileType,
                        tileOptions = tileOptions,
                        tileEntityPrefabPath = tileEntityPath
                    };
                    idMap.Add(tileData.id, tileData);
                    break;
                case "Conduit":
                    string conduitType = (string) jsonObject["conduitType"];
                    string ruleTilePath = (string) jsonObject["ruleTilePath"];
                    int[] colorArray = JsonConvert.DeserializeObject<int[]>(jsonObject["color"].ToString());
                    int hardness = JsonConvert.DeserializeObject<int>(jsonObject["hardness"].ToString());
                    int speed = JsonConvert.DeserializeObject<int>(jsonObject["speed"].ToString());
                    ConduitData conduitData = new ConduitData {
                        id=id,
                        spritePath=spritePath,
                        name=name,
                        conduitType=conduitType,
                        ruleTilePath=ruleTilePath,
                        hardness = hardness,
                        color = new Color(colorArray[0]/255f,colorArray[1]/255f,colorArray[2]/255f),
                        speed = speed,
                        conduitOptions = new ConduitOptions()
                    };
                    idMap.Add(conduitData.id, conduitData);
                    break;
                case "Matter":
                    
                    break;
            }
        }
    }
    public static IdDataMap getInstance() {
        if (idDataMap == null) {
            idDataMap = new IdDataMap();
        }
        return idDataMap;
    }

    public IdData GetIdData(int id) {
        return idMap[id];
    }
    public Sprite GetSprite(int id) {
        string[] splitSpritePath = idMap[id].spritePath.Split("|");
        if (splitSpritePath.Length == 1) {
            return Resources.Load<Sprite>(splitSpritePath[0]);
        } else if (splitSpritePath.Length == 2) {
            foreach (Sprite subSprite in Resources.LoadAll<Sprite>(splitSpritePath[0])) {
                if (subSprite.name == splitSpritePath[1]) {
                    return subSprite;
                }
            }
        }
        return null;
    }

    public TileData getIdTileData(int id) {
        if (idMap[id] is TileData) {
            return (TileData) idMap[id];
        }
        return null;
        
    }

    public ConduitData getIdConduitData(int id) {
        if (idMap[id] is ConduitData) {
            return (ConduitData) idMap[id];
        }
        return null;
    }   

    public RuleTile getIdRuleTile(int id) {
        return Resources.Load<RuleTile>(idMap[id].spritePath);
    }

    public TileData copyTileData(int id) {
        TileData tileData = getIdTileData(id);
        TileData copy = new TileData {
            id = id,
            spritePath = tileData.spritePath,
            name = tileData.name,
            dataType = tileData.dataType,
            tileType = tileData.tileType,
            tileOptions = SDictionary.copy(tileData.tileOptions),
            tileEntityPrefabPath = tileData.tileEntityPrefabPath
        };
        return copy;
    }
}
