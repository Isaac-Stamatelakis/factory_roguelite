using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Tiles {
    [System.Serializable]
    public struct DynamicTileOptions {
        public bool hitable;
        public int hardness;
        public bool rotatable;
        public bool hasStates;
        public List<DropOption> dropOptions;
    }
    [System.Serializable]
    public struct SerializedTileOptions {
        public int rotation;
        public int state;
    }
    [System.Serializable]
    public class TileOptions {
        [SerializeField] [JsonProperty] private SerializedTileOptions serializedTileOptions;
        [SerializeField] private DynamicTileOptions tileOptions;
        [JsonIgnore] public SerializedTileOptions SerializedTileOptions { get => serializedTileOptions; set => serializedTileOptions = value; }
        [JsonIgnore] public DynamicTileOptions DynamicTileOptions { get => tileOptions; set => tileOptions = value; }
        public TileOptions(SerializedTileOptions serializedTileOptions, DynamicTileOptions dynamicTileOptions) {
            this.tileOptions = dynamicTileOptions;
            this.serializedTileOptions = serializedTileOptions;
        }
    } 
    [System.Serializable]
    public class DropOption {
        public string id;
        public int weight;
    }
    
    public static class TileOptionFactory {
        public static string serialize(TileOptions tileOptions) {
            return JsonConvert.SerializeObject(tileOptions);
        }
        public static TileOptions deserialize(string data, TileItem tileItem) {
            if (data == null) {
                return new TileOptions(
                    tileItem.tileOptions.SerializedTileOptions,
                    tileItem.tileOptions.DynamicTileOptions
                );
            }
            try {
                return new TileOptions(
                    JsonConvert.DeserializeObject<SerializedTileOptions>(data),
                    tileItem.tileOptions.DynamicTileOptions
                );
            } catch (JsonSerializationException ex) {
                Debug.LogError("TileOptionFactory method 'deserialize' error: " + ex);
                return getDefault(tileItem);
            }
            
        }
        public static TileOptions getDefault(TileItem tileItem) {
            return new TileOptions(
                tileItem.tileOptions.SerializedTileOptions,
                tileItem.tileOptions.DynamicTileOptions
            );
        }
    }

    
}